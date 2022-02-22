using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebStore.Controllers.Common
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        public readonly ILogger _logger;
        public ErrorController(ILogger logger)
        {
            _logger = logger;
        }

        [Route("/error")]
        public IActionResult Error()
        {
            var clientIP = HttpContext.Connection?.RemoteIpAddress.MapToIPv4();
            var clientDNS = Dns.GetHostEntry(clientIP);

            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var statusCode = exception.Error.GetType().Name switch
            {
                "ArgumentException" => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.ServiceUnavailable
            };

            _logger.Information($"Client '{clientDNS.HostName}' with ip '{clientIP}'");
            _logger.Error($"An exception was caught with code '{(int)statusCode} - {statusCode}' and message '{exception.Error.Message}'");

            return Problem(detail: exception.Error.Message, statusCode: (int)statusCode);
        }
    }
}