using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace dotnetcore.Controllers
{
    [Route("api/[controller]/{containerName}")]
    public class BlobStorageController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly CloudBlobClient _blobClient;

        public BlobStorageController(IConfiguration configuration)
        {
            _configuration = configuration;
            _blobClient = CloudStorageAccount.Parse(_configuration["StorageConnectionString"])
                                             .CreateCloudBlobClient();
        }

        private CloudBlobContainer GetContainer()
        {
            var blobContainer = _blobClient.GetContainerReference(ContainerName());
            blobContainer.CreateIfNotExistsAsync().Wait();
            return blobContainer;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            BlobContinuationToken token = null;
            var data = GetContainer().ListBlobsSegmentedAsync(token);

            return data.Result.Results.OfType<CloudBlockBlob>().Select(x => x.Name);
        }

        [HttpGet("{blobName}")]
        public async Task<CreditNoteDocument> Get(string blobName)
        {
            var blockBlob = GetContainer().GetBlockBlobReference(blobName);

            var contents = await blockBlob.DownloadTextAsync();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<CreditNoteDocument>(contents);

            return data;
        }

        [HttpPost]
        public Task Post()
        {
            var data = new CreditNoteDocument()
            {
                 AccountId = Environment.TickCount
            };
            data.Transactions.Add(3);

            var contents = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var blockBlob = GetContainer().GetBlockBlobReference("blob-" + Guid.NewGuid());

            return blockBlob.UploadTextAsync(contents);
        }

        [HttpDelete]
        public async void Delete()
        {
            var blobs = await GetContainer().ListBlobsSegmentedAsync(null);

            foreach (var blob in blobs.Results)
            {
                if (blob.GetType() == typeof(CloudBlockBlob))
                {
                    var blockBlob = (CloudBlockBlob)blob;
                    await blockBlob.DeleteAsync();
                }
            }
        }

        private string ContainerName()
        {
            return (string)this.RouteData.Values.GetValueOrDefault("containerName");
        }
    }


    public class CreditNoteDocument
    {
        public CreditNoteDocument()
        {
            Transactions = new List<long>();
        }

        public long AccountId { get; set; }

        public List<long> Transactions { get; set; }
    }
}