using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public interface IResponseJsonHandler
    {
        Task SendResponseMessageAsync(HttpResponseDataModel httpResponseData, string json);
    }
}
