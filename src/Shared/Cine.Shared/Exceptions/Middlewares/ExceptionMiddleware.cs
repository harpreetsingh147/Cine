using System;
using System.Threading.Tasks;
using Cine.Shared.Exceptions.Mappers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Cine.Shared.Exceptions.Middlewares
{
    internal sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IExceptionCompositionRoot _exceptionCompositionRoot;

        public ExceptionMiddleware(RequestDelegate next, IExceptionCompositionRoot exceptionCompositionRoot)
        {
            _next = next;
            _exceptionCompositionRoot = exceptionCompositionRoot;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                var responseData = _exceptionCompositionRoot.Map(ex);

                if (responseData is { httpStatusCode: var httpStatusCode, errorCodes: var errorCodes})
                {
                    var json = JsonConvert.SerializeObject(new { Errors = errorCodes });
                    httpContext.Response.StatusCode = httpStatusCode;
                    httpContext.Response.Headers.Add("content-type", "application/json");
                    await httpContext.Response.WriteAsync(json);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
