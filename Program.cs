using BibliotecaMVC.Controllers;
using BibliotecaMVC.Data;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ErrorDeDatosFilter>();
    // Mensajes de enlace de modelo en español (por ejemplo, texto en un campo numérico).
    var mensajes = options.ModelBindingMessageProvider;
    mensajes.SetValueMustBeANumberAccessor(campo => $"El campo {campo} debe ser un número.");
    mensajes.SetValueMustNotBeNullAccessor(campo => $"El campo {campo} es obligatorio.");
    mensajes.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
    mensajes.SetMissingBindRequiredValueAccessor(campo => $"El campo {campo} es obligatorio.");
});
builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibliotecaDB")));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddIdentityCookies();
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<BibliotecaContext>()
    .AddSignInManager()
    .AddErrorDescriber<IdentityErrores>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccesoDenegado";
    options.Cookie.Name = "BibliotecaMVC.Identity";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});
builder.Services.AddScoped<IImagenService, ImagenService>();
builder.Services.AddScoped<IRepositorioLibro, RepositorioLibrosEf>();
builder.Services.AddScoped<IRepositorioAutor, RepositorioAutoresEf>();
builder.Services.AddScoped<ILibroService, LibroService>();
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<IRepositorioCategoria, RepositorioCategoriasSql>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

var app = builder.Build();

// Migraciones, roles y cuentas iniciales. Si SQL Server no está disponible, la aplicación arranca igual
// y los módulos con datos muestran la página de error controlada.
using (var scope = app.Services.CreateScope())
{
    try
    {
        await InicializadorIdentidad.InicializarAsync(scope.ServiceProvider, app.Configuration);
    }
    catch (Exception ex) when (ex is SqlException or DbUpdateException or InvalidOperationException or ArgumentException)
    {
        app.Logger.LogError(ex, "No se pudo inicializar la base de datos ni los roles.");
    }
}

// Configure the HTTP request pipeline.
// Development muestra la página de excepciones del desarrollador; en producción UseExceptionHandler centraliza
// las excepciones no controladas en /Home/Error, que solo muestra un mensaje general y un código de referencia.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Respuestas sin contenido (NotFound(), BadRequest(), rutas inexistentes) muestran una página propia en lugar de
// la pantalla en blanco del navegador. Conserva el código HTTP original.
app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");

app.UseHttpsRedirection();
// Sirve las portadas subidas en tiempo de ejecución (wwwroot/images/libros), que MapStaticAssets no conoce.
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
