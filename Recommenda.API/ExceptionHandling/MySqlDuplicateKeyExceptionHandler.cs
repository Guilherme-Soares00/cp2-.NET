using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Recommenda.API.ExceptionHandling;

public sealed class MySqlDuplicateKeyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException)
            return false;

        var cause = exception.InnerException;
        while (cause is not null)
        {
            if (cause is MySqlException { Number: 1062 })
            {
                await Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Registro duplicado.",
                    detail: "Já existe um registro com o mesmo valor em um campo que deve ser único.")
                    .ExecuteAsync(httpContext);
                return true;
            }

            cause = cause.InnerException;
        }

        return false;
    }
}
