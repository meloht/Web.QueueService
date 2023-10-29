using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.QueueService.BackendAPI.Models
{
    public class JsonModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }


        public string Ip { get; set; }
    }
}
