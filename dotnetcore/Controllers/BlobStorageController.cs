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
        public object Get(string blobName)
        {
            return blobName;
        }

        [HttpPost]
        public Task Post()
        {
            var blockBlob = GetContainer().GetBlockBlobReference("blob-" + Guid.NewGuid());

            return blockBlob.UploadTextAsync(blockBlob.Name);
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


    public class CreditNoteDocument : TableEntity
    {
        public CreditNoteDocument(string pk, string rk)
        {
            PartitionKey = pk;
            RowKey = rk;
        }

        public CreditNoteDocument()
        {
        }

        public long AccountId { get; set; }

        public List<long> Transactions { get; set; }
    }
}