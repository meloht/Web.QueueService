using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class ResponseHandlerBase
    {
        protected static readonly object objLock = new object();
        protected AppSettingsWorkerJob AppSettings;
        protected static StorageBlobClient blobClient = null;
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        public ResponseHandlerBase(AppSettingsWorkerJob appSettings)
        {
            AppSettings = appSettings;

        }

        protected void InitBlob(AppSettingsWorkerJob appSettings)
        {
            if (blobClient == null)
            {
                lock (objLock)
                {
                    if (blobClient == null)
                    {

                        blobClient = new StorageBlobClient(appSettings.StorageBlobConnectionString, appSettings.BlobContainName);
                    }
                }
            }
        }
    }
}
