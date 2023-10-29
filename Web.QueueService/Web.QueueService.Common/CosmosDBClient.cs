using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class CosmosDBClient : IDisposable
    {
        private readonly string DatabaseName;
        private readonly string CollectionName;

        // Read config
        private readonly string EndpointUrl;
        private readonly string AuthorizationKey;
        private DocumentClient Client;

        private Logger logger = LogManager.GetCurrentClassLogger();

        public CosmosDBClient(CosmosDbModel config)
        {
            try
            {
                DatabaseName = config.DatabaseName;
                CollectionName = config.CollectionName;
                EndpointUrl = config.EndpointUrl;
                AuthorizationKey = config.AuthorizationKey;

                Client = new DocumentClient(new Uri(EndpointUrl), this.AuthorizationKey);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }
        public static void Init(CosmosDbModel config)
        {
            using (DocumentClient client = new DocumentClient(new Uri(config.EndpointUrl), config.AuthorizationKey))
            {
                CreateDatabaseIfNotExistsAsync(client, config).Wait();
                CreateCollectionIfNotExistsAsync(client, config).Wait();

            }
        }


        private static async Task CreateDatabaseIfNotExistsAsync(DocumentClient client, CosmosDbModel config)
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(config.DatabaseName));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = config.DatabaseName });
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync(DocumentClient client, CosmosDbModel config)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(config.DatabaseName, config.CollectionName));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(config.DatabaseName),
                        new DocumentCollection
                        {
                            Id = config.CollectionName
                        },
                        new RequestOptions { OfferThroughput = 400 });
                }

            }
        }

        public T GetItem<T>(string id)
        {
            try
            {
                Document document =
                     Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseName, CollectionName, id)).GetAwaiter().GetResult();
                return (T)(dynamic)document;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return default(T);
        }

        public async Task<List<T>> GetItemListAsync<T>(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = Client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName),
                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<Document> CreateItemAsync<T>(T item)
        {
            try
            {
                var doc = await Client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName), item);
                return doc;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return null;
        }

        public void DeleteItem(string id)
        {
            try
            {
                Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseName, CollectionName, id)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void Dispose()
        {
            try
            {
                Client.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }
    }
}
