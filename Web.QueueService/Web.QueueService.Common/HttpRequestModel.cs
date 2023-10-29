using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class HttpRequestModel
    {
        public string ContentType { get; set; }
        public bool IsHttps { get; set; }

        public string Method { get; set; }

        public string Body { get; set; }

        public Dictionary<string, string[]> FormValues { get; set; }

        public string Guid { get; set; }

        public string TargetUrl { get; set; }

        public Dictionary<string, string[]> QueryStringValues { get; set; }
    }
}
