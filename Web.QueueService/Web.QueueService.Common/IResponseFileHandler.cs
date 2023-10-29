using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public interface IResponseFileHandler
    {
        Task SendResponseMessageAsync(HttpResponseDataModel httpResponseData, Stream stream);
    }
}
