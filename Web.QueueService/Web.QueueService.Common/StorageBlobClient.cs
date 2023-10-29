using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class StorageBlobClient
    {
        private readonly string StorageConnectionString;
        private readonly string BlobContainerName;
        private CloudBlobClient CloudBlobClient;
        private CloudBlobContainer CloudBlobContainer;
        private Logger logger = LogManager.GetCurrentClassLogger();
        public StorageBlobClient(string conn, string containerName)
        {
            try
            {
                StorageConnectionString = conn;
                BlobContainerName = containerName;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

                CloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer = CloudBlobClient.GetContainerReference(containerName);


                // Set the permissions so the blobs are public. 
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                CloudBlobContainer.SetPermissions(permissions);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        public static void Init(string conn, string containerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(conn);

            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();

            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            container.SetPermissions(permissions);
        }

        public string UploadFile(Stream stream, string guid)
        {
            try
            {
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(guid);
                cloudBlockBlob.UploadFromStream(stream);
                return guid;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return null;

        }

        public async Task UploadStringAsync(string json, string guid)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            byte[] jsonbs = System.Text.Encoding.UTF8.GetBytes(json);
            using (var stream = new MemoryStream(jsonbs))
            {
                try
                {
                    CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(guid);
                    await cloudBlockBlob.UploadFromStreamAsync(stream);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }

            }
            sw.Stop();

            logger.Info($"QueueID:{guid} Blob UploadStringAsync executionTime:{sw.Elapsed.ToString()}");
        }

        public async Task<string> DownloadStringAsync(string guid)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            using (var stream = new MemoryStream())
            {
                try
                {
                    CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(guid);
                    await cloudBlockBlob.DownloadToStreamAsync(stream);
                    stream.Position = 0;
                    StreamReader sr = new StreamReader(stream);
                    string json = sr.ReadToEnd();
                    sr.Close();

                    return json;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            sw.Stop();

            logger.Info($"QueueID:{guid} Blob DownloadStringAsync executionTime:{sw.Elapsed.ToString()}");
            return null;
        }
        public async Task UploadFileAsync(Stream stream, string guid)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {

                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(guid);
                await cloudBlockBlob.UploadFromStreamAsync(stream);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            sw.Stop();

            logger.Info($"QueueID:{guid} Blob UploadFileAsync executionTime:{sw.Elapsed.ToString()}");

        }
        public void GetFile(Stream stream, string guid)
        {
            try
            {
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(guid);
                cloudBlockBlob.DownloadToStream(stream);
                cloudBlockBlob.DeleteIfExists();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        public void DownLoadFile(Stream stream, string guid)
        {
            try
            {
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(guid);
                cloudBlockBlob.DownloadToStream(stream);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        public void DeleteFile(string guid)
        {
            try
            {
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(guid);
                cloudBlockBlob.DeleteIfExists();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public async Task DeleteFileAsync(string guid)
        {
            try
            {
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(guid);
                await cloudBlockBlob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }


    }
}
