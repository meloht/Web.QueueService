using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public interface IQueueService
    {
        Task ProcessRequestAsync(HttpContext httpContext, AppSettingsQueueApi appSettings);
    }
}
