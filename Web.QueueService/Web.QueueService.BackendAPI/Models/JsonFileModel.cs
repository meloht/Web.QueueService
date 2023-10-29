using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.QueueService.BackendAPI.Models
{
    public class JsonFileModel
    {
        public string FileSize { get; set; }
        public string UploadFileTime { get; set; }

        public string DownLoadFileTime { get; set; }

        public string SendMessageQueueTime { get; set; }

         
    }
}
