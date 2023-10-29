using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.QueueService.BackendAPI.Models
{
    public class JsonDocModel
    {
        public string id { get; set; }

        public List<string> List { get; set; }
    }

    public class JsonTimeModel
    {
        public string id { get; set; }

        public List<DateTime> List { get; set; }
    }
}
