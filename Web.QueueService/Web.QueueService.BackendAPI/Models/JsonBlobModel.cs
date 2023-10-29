using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.QueueService.BackendAPI.Models
{
    public class JsonBlobModel
    {
        public string JsonSize { get; set; }
        public string UploadJsonToBlobTime { get; set; }

        public string DownloadJsonFromBlobTime { get; set; }
    }
}
