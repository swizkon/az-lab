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
    [Route("api/[controller]")]
    public class BlobStorageController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly CloudBlobContainer _blobContainer;

        public BlobStorageController(IConfiguration configuration)
        {
            _configuration = configuration;
            var account = CloudStorageAccount.Parse(configuration["StorageConnectionString"]);
            var blobClient = account.CreateCloudBlobClient();

            _blobContainer = blobClient.GetContainerReference("blobcontainer");
            _blobContainer.CreateIfNotExistsAsync().Wait();

            _blobContainer.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            }).Wait();
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            BlobContinuationToken token = null;
            var data = _blobContainer.ListBlobsSegmentedAsync(token);
            
            return data.Result.Results.OfType<CloudBlockBlob>().Select(x => x.Name);
        }

        [HttpGet("{resource}")]
        public object Get(string resource)
        {
            return _configuration.AsEnumerable().OrderBy(k => k.Key).Select(k => k.Key);
        }

        [HttpPost]
        public Task Post()
        {
            var blockBlob = _blobContainer.GetBlockBlobReference("blob-" + Guid.NewGuid());

            return blockBlob.UploadTextAsync(blockBlob.Name);
        }

        [HttpDelete]
        public async void Delete()
        {
            var blobs = await _blobContainer.ListBlobsSegmentedAsync(null);

            foreach (var blob in blobs.Results)
            {
                if (blob.GetType() == typeof(CloudBlockBlob))
                {
                    var blockBlob = (CloudBlockBlob) blob;
                    await blockBlob.DeleteAsync();
                }
            }
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