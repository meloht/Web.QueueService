using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class HttpResponseDataModel
    {
        public string Guid { get; set; }
        public string ContentType { get; set; }

        public string Json { get; set; }

        public ResultType ResultType { get; set; }

        public Attachment Attachment { get; set; }

    }

    public class Attachment
    {
        public string[] ContentDisposition { get; set; }
        public string BlobGuid { get; set; }
    }

    public enum ResultType
    {
        Json = 0,
        Blob = 1,
        Redis = 2,
        File = 3
    }
}
