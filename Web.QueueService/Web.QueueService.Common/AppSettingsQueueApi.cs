using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class AppSettingsQueueApi : AppSettingsBase
    {
        public AppSettingsQueueApi(IConfiguration config) : base(config)
        {
            targetUrl = config["TargetUrl"];

        }
        private string targetUrl;
        public string TargetUrl
        {
            get { return targetUrl; }
        }
    }
}
