"""Run after dotnet build; use only a dedicated BibliotecaMVC_Test_ database."""
import html
import http.cookiejar
import os
from pathlib import Path
import re
import subprocess
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
BASE = "http://localhost:5198"
CONNECTION = os.environ.get("BIBLIOTECA_TEST_CONNECTION", "")
if not re.search(r"(?:Database|Initial Catalog)\s*=\s*BibliotecaMVC_Test_[A-Za-z0-9_]+(?:;|$)", CONNECTION, re.I):
    raise SystemExit("Set BIBLIOTECA_TEST_CONNECTION to a dedicated BibliotecaMVC_Test_ database.")

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None

cookies = http.cookiejar.CookieJar()
client = urllib.request.build_opener(urllib.request.ProxyHandler({}),
                                    urllib.request.HTTPCookieProcessor(cookies), NoRedirect)
checks = 0
process = None
log = tempfile.TemporaryFile()
test_user = "identity_test_" + str(time.time_ns())
test_email = test_user + "@example.test"

def identity_sql(cleanup=False):
    # Only inspect/delete the exact temporary account in the dedicated test database.
    script = r'''
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$connection = New-Object System.Data.SqlClient.SqlConnection $env:BIBLIOTECA_TEST_CONNECTION
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = if ($env:BIBLIOTECA_IDENTITY_CLEANUP -eq '1') {
        'DELETE FROM dbo.AspNetUsers WHERE UserName = @user'
    } else {
        'SELECT COUNT(*) FROM dbo.AspNetUsers WHERE UserName = @user AND PasswordHash IS NOT NULL AND LEN(PasswordHash) > 50'
    }
    [void]$command.Parameters.Add('@user', [Data.SqlDbType]::NVarChar, 256)
    $command.Parameters['@user'].Value = $env:BIBLIOTECA_IDENTITY_TEST_USER
    if ($env:BIBLIOTECA_IDENTITY_CLEANUP -eq '1') { [void]$command.ExecuteNonQuery() }
    else { $command.ExecuteScalar() }
} finally { $connection.Dispose() }
'''
    result = subprocess.run(["powershell.exe", "-NoProfile", "-Command", script],
                            env={**os.environ, "BIBLIOTECA_IDENTITY_TEST_USER": test_user,
                                 "BIBLIOTECA_IDENTITY_CLEANUP": "1" if cleanup else "0"},
                            check=True, capture_output=True, text=True)
    return result.stdout.strip()

def authenticated():
    body = request("/Home/Usuarios")[1]
    return test_user in body and 'action="/Account/Logout"' in body

def identity_cookie():
    return next((cookie for cookie in cookies if cookie.name == "BibliotecaMVC.Identity"), None)

def authentication_checks():
    account = {"Usuario": test_user, "Email": test_email,
               "Password": "Prueba-Identity9!", "ConfirmarPassword": "Prueba-Identity9!"}
    check(not authenticated(), "Initially anonymous")
    check(request("/Account/Register", account)[0] == 400, "Register requires antiforgery")
    for invalid, message in (
        ({**account, "Usuario": ""}, "Ingresa un nombre"),
        ({**account, "Usuario": "otro@example.test"}, "Usa letras"),
        ({**account, "Email": "invalid"}, "correo electr"),
        ({**account, "ConfirmarPassword": "Otra-Password9!"}, "no coinciden"),
        ({**account, "Password": "abcdefgh", "ConfirmarPassword": "abcdefgh"}, "mayúscula"),
    ):
        status, body, _ = post("/Account/Register", invalid)
        check(status == 200 and message in html.unescape(body), "Register rejects invalid input: " + message)
        check(not authenticated(), "Invalid registration does not authenticate")
    status, _, headers = post("/Account/Register", {**account, "ReturnUrl": "https://example.test/"})
    check(status == 302 and headers["Location"] == "/", "Registration rejects external return URL")
    check(authenticated(), "Registration signs in")
    cookie = identity_cookie()
    check(cookie is not None and cookie.discard and cookie.expires is None, "Registration session cookie")
    check(identity_sql() == "1", "Exactly one user stored with password hash")
    check(request("/Account/Logout")[0] in (404, 405) and authenticated(), "GET cannot log out")
    check(request("/Account/Logout", {})[0] == 400 and authenticated(), "Logout requires antiforgery")
    check(post("/Account/Logout", {}, "/Home/Usuarios")[0] == 302 and not authenticated(), "POST logs out")
    check(identity_cookie() is None, "Logout removes identity cookie")
    for duplicate, message in (
        ({**account, "Email": "another@example.test"}, "nombre de usuario ya"),
        ({**account, "Usuario": test_user + "_other", "Email": test_email.upper()}, "correo electrónico ya"),
    ):
        status, body, _ = post("/Account/Register", duplicate)
        check(status == 200 and message in html.unescape(body) and not authenticated(), "Duplicate rejected: " + message)
    login = {"UsuarioOCorreo": test_user, "Password": account["Password"]}
    check(request("/Account/Login", login)[0] == 400, "Login requires antiforgery")
    for invalid in ({**login, "UsuarioOCorreo": ""}, {**login, "Password": ""}):
        status, body, _ = post("/Account/Login", invalid)
        check(status == 200 and "field-validation-error" in body, "Required Login fields")
    failures = []
    for identifier in (test_user, "missing@example.test"):
        status, body, _ = post("/Account/Login", {"UsuarioOCorreo": identifier, "Password": "Wrong-Password9!"})
        check(status == 200 and not authenticated(), "Invalid credentials do not sign in")
        failures.append(re.search(r'<div[^>]*auth-validation[^>]*>(.*?)</div>', body, re.S).group(1))
    check(failures[0] == failures[1], "Same credential error for known and unknown user")
    status, _, headers = post("/Account/Login", {**login, "UsuarioOCorreo": test_user.upper(), "ReturnUrl": "/Libros"})
    check(status == 302 and headers["Location"] == "/Libros" and authenticated(), "Username Login accepts local return URL")
    check(identity_cookie().discard, "Login without remember uses session cookie")
    post("/Account/Logout", {}, "/Home/Usuarios")
    status, _, headers = post("/Account/Login", {**login, "UsuarioOCorreo": test_email.upper(), "Recordarme": "true", "ReturnUrl": "//example.test"})
    check(status == 302 and headers["Location"] == "/" and authenticated(), "Email Login rejects protocol-relative redirect")
    cookie = identity_cookie()
    check(not cookie.discard and cookie.expires > time.time(), "Remember creates persistent cookie")
    stop()
    start()
    check(authenticated(), "Session survives application restart")
    post("/Account/Logout", {}, "/Home/Usuarios")
    cookies.clear()
    check(post("/Account/Login", login)[0] == 302 and authenticated(), "Stored account signs in after restart without previous cookies")
    post("/Account/Logout", {}, "/Home/Usuarios")
    for _ in range(5):
        check(post("/Account/Login", {**login, "Password": "Wrong-Password9!"})[0] == 200, "Count failed login")
    status, body, _ = post("/Account/Login", login)
    check(status == 200 and "espera 5 minutos" in body and not authenticated(), "Locked account rejects correct password")

def check(condition, label):
    global checks
    if not condition:
        raise AssertionError(label)
    checks += 1

def request(path, data=None):
    body = None if data is None else urllib.parse.urlencode(data).encode()
    try:
        response = client.open(BASE + path, body, timeout=15)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        return response.status, response.read().decode(), response.headers

def token(path):
    status, body, _ = request(path)
    check(status == 200, "Form GET " + path)
    match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)
    check(match is not None, "Antiforgery token " + path)
    return html.unescape(match.group(1))

def post(path, data, form=None):
    return request(path, {**data, "__RequestVerificationToken": token(form or path)})

def stop():
    global process
    if process is not None:
        process.terminate()
        process.wait(timeout=15)
        process = None

def start(connection=CONNECTION):
    global process
    env = {**os.environ, "ASPNETCORE_ENVIRONMENT": "Development",
           "ASPNETCORE_URLS": BASE, "ConnectionStrings__BibliotecaDB": connection}
    process = subprocess.Popen(["dotnet", str(ROOT / "bin/Debug/net10.0/BibliotecaMVC.dll")],
                               cwd=ROOT, env=env, stdout=log, stderr=log)
    for _ in range(100):
        if process.poll() is not None:
            raise RuntimeError("Application failed to start; inspect local dotnet configuration.")
        try:
            if request("/")[0] == 200:
                return
        except (urllib.error.URLError, ConnectionError, TimeoutError):
            pass
        time.sleep(0.1)
    raise RuntimeError("Application startup timeout")

try:
    start()
    authentication_checks()
    check(request("/Home/Categorias")[0] == 302, "Legacy category route redirects")
    marker = "Prueba " + str(time.time_ns())
    cases = [
        ("Libros", {"Titulo": marker, "Autor": "Autora", "Categoria": "Novela", "AnioPublicacion": "2020", "ISBN": "978-123", "Imagen": "ficciones.png", "Descripcion": "", "Disponible": "true"}, "Titulo"),
        ("Autores", {"Nombre": marker, "Apellido": "García", "Nacionalidad": "Guatemala", "FechaNacimiento": "2000-01-01", "Activo": "true"}, "Nombre"),
        ("Categorias", {"Nombre": marker, "Descripcion": ""}, "Nombre"),
    ]
    for controller, values, title_key in cases:
        base = "/" + controller
        check(request(base)[0] == 200, "Index " + controller)
        check(request(base + "/Create", values)[0] == 400, "Reject missing antiforgery " + controller)
        invalid = {**values, title_key: ""}
        status, body, _ = post(base + "/Create", invalid)
        check(status == 200 and "field-validation-error" in body, "Required validation " + controller)
        if controller == "Libros":
            status, body, _ = post(base + "/Create", {**values, "AnioPublicacion": "2099"})
            check(status == 200 and "futuro" in body, "Future publication HTTP validation")
            status, body, _ = post(base + "/Create", {**values, "Imagen": "../invalid.png"})
            check(status == 200 and "field-validation-error" in body, "Image HTTP validation")
        elif controller == "Autores":
            status, body, _ = post(base + "/Create", {**values, "FechaNacimiento": "2099-01-01"})
            check(status == 200 and "futuro" in body, "Future birth HTTP validation")
        else:
            for invalid in ({**values, "Nombre": "n" * 101}, {**values, "Descripcion": "d" * 251}):
                status, body, _ = post(base + "/Create", invalid)
                check(status == 200 and "field-validation-error" in body, "Category length HTTP validation")
        status, _, headers = post(base + "/Create", {**values, "ID": "2147483647"})
        check(status == 302 and headers["Location"] == base, "Create redirects " + controller)
        _, listing, _ = request(base)
        blocks = re.findall(r'<(?:article|tr)\b[^>]*>.*?</(?:article|tr)>', html.unescape(listing), re.S)
        created = next((block for block in blocks if marker in block), "")
        match = re.search(r'href="' + base + r'/Details/(\d+)"', created)
        check(match is not None, "Created row appears " + controller)
        row_id = match.group(1)
        check(row_id != "2147483647", "Create ignores supplied ID " + controller)
        check(request(base + "/Details/" + row_id)[0] == 200, "Details " + controller)
        status, _, _ = post(base + "/Edit/" + row_id, {**values, "ID": "2147483647"})
        check(status == 400, "Route/body mismatch " + controller)
        values = {**values, "ID": row_id, title_key: marker + " editado"}
        check(post(base + "/Edit/" + row_id, values)[0] == 302, "Edit " + controller)
        check("editado" in html.unescape(request(base + "/Details/" + row_id)[1]), "Edit persisted " + controller)
        check(request(base + "/Delete/" + row_id)[0] == 200, "Delete confirmation " + controller)
        check(request(base + "/Details/" + row_id)[0] == 200, "GET Delete must not delete " + controller)
        stop()
        start()
        check("editado" in html.unescape(request(base + "/Details/" + row_id)[1]), controller + " survives application restart")
        delete_token = token(base + "/Delete/" + row_id)
        check(request(base + "/Delete/" + row_id, {"id": row_id, "__RequestVerificationToken": delete_token})[0] == 302, "Delete POST " + controller)
        check(request(base + "/Delete/" + row_id, {"id": row_id, "__RequestVerificationToken": delete_token})[0] == 404, "Repeated delete " + controller)
        for action in ("Details", "Edit", "Delete"):
            check(request(base + "/" + action + "/" + row_id)[0] == 404, "Missing row " + controller + action)
        check(post(base + "/Edit/" + row_id, values, base + "/Create")[0] == 404, "Stale edit " + controller)
    stop()
    for invalid_connection in ("", "invalid-secret-marker", "Server=127.0.0.1,1;Database=missing;Integrated Security=True;Connect Timeout=1;Encrypt=True;TrustServerCertificate=True"):
        start(invalid_connection)
        status, body, _ = request("/Categorias")
        check(status == 503 and "No se pudo completar" in body, "Controlled database failure")
        check("invalid-secret-marker" not in body and "SqlException" not in body, "No database diagnostics in response")
        for controller in ("Libros", "Autores"):
            status, body, _ = request("/" + controller)
            check(status == 503 and "No se pudo completar" in body, "Controlled EF failure " + controller)
            check("invalid-secret-marker" not in body and "SqlException" not in body, "No EF diagnostics " + controller)
        check(request("/")[0] == 200, "Home still available without SQL")
        check(post("/Categorias/Create", {"Nombre": marker})[0] == 503, "Failed write does not redirect to success")
        for action, values in (("Login", {"UsuarioOCorreo": test_user, "Password": "Prueba-Identity9!"}),
                               ("Register", {"Usuario": test_user, "Email": test_email, "Password": "Prueba-Identity9!", "ConfirmarPassword": "Prueba-Identity9!"})):
            status, body, _ = post("/Account/" + action, values)
            check(status == 503 and "No se pudo completar" in body, "Controlled Identity database failure " + action)
            check("invalid-secret-marker" not in body and "SqlException" not in body, "No Identity diagnostics " + action)
        stop()
    print(f"PASS: {checks} HTTP checks, including SQL persistence across restart.")
except Exception:
    log.seek(0)
    (ROOT / "tests/http_smoke.log").write_bytes(log.read())
    raise
finally:
    stop()
    log.close()
    identity_sql(cleanup=True)
