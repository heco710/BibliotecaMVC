using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Data.SqlClient;

namespace BibliotecaMVC.Controllers;

public sealed class ErrorDeDatosFilter(ILogger<ErrorDeDatosFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor action ||
            action.ControllerTypeInfo.AsType() != typeof(CategoriasController)) return;
        if (context.Exception is not (SqlException or InvalidOperationException or ArgumentException)) return;

        // Log the error category, not connection strings or user-submitted values.
        logger.LogError("Error de datos en Categorias: {Tipo}; código SQL: {Codigo}",
            context.Exception.GetType().Name, (context.Exception as SqlException)?.Number);
        context.Result = new ViewResult { ViewName = "ErrorDeDatos", StatusCode = StatusCodes.Status503ServiceUnavailable };
        context.ExceptionHandled = true;
    }
}
