using Microsoft.AspNetCore.Hosting;
using Web.QueueService.Common;

namespace Web.QueueService.API.Middleware
{
    public class RequestHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        private IQueueService _queueService;
        public RequestHandlerMiddleware(RequestDelegate next, IQueueService queueService)
        {
            _next = next;
            _queueService = queueService;
        }


        public async Task Invoke(HttpContext httpContext)
        {
            await _queueService.ProcessRequestAsync(httpContext, RequestMiddlewareExtensions.Config);
        }
    }
}
