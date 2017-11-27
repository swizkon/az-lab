using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace dotnetcore.Controllers
{
    [Route("api/[controller]")]
    public class TableStorageController : Controller
    {
        private readonly CloudTable _testTable;

        public TableStorageController(IConfiguration configuration)
        {
            var account = CloudStorageAccount.Parse(configuration["StorageConnectionString"]);
            var tableClient = account.CreateCloudTableClient();

            _testTable = tableClient.GetTableReference("Test");
            _testTable.CreateIfNotExistsAsync().Wait();
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var result = new List<string>();

            var query = new TableQuery<CreditNoteTransaction>();

            TableContinuationToken token = null;
            do
            {
                var resultSegment = await _testTable.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                result.AddRange(resultSegment.Results.Select(entity => entity.PartitionKey + ", " + entity.RowKey));
            } while (token != null);

            return result;
        }

        [HttpGet("{partition}/{row}")]
        public async Task<CreditNoteTransaction> Get(string partition, string row)
        {
            var retrieveOperation = TableOperation.Retrieve<CreditNoteTransaction>(partition, row);
            var resultSegment = await _testTable.ExecuteAsync(retrieveOperation);

            return resultSegment.Result as CreditNoteTransaction;
        }

        [HttpPost]
        public Task<TableResult> Post(string partition = "myPartitionKey", long transactionId = 0, long accountId = 0)
        {
            var row = new CreditNoteTransaction(partition, Guid.NewGuid().ToString())
            {
                AccountId = accountId,
                TransactionId = transactionId
            };
            return _testTable.ExecuteAsync(TableOperation.Insert(row));
        }
        
        [HttpDelete("{partition}/{row}")]
        public async Task<TableResult> Delete(string partition, string row)
        {
            var entity = await Get(partition, row);

            return await _testTable.ExecuteAsync(TableOperation.Delete(entity));
        }

        [HttpDelete]
        public async void Delete()
        {
            var query = new TableQuery<CreditNoteTransaction>();

            TableContinuationToken token = null;
            do
            {
                var resultSegment = await _testTable.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                foreach (var entity in resultSegment.Results)
                {
                    _testTable.ExecuteAsync(TableOperation.Delete(entity)).Wait();
                }
            } while (token != null);
        }
    }

    public class CreditNoteTransaction : TableEntity
    {
        public CreditNoteTransaction(string pk, string rk)
        {
            PartitionKey = pk;
            RowKey = rk;
        }

        public CreditNoteTransaction()
        {
        }

        public long AccountId { get; set; }

        public long TransactionId { get; set; }
    }
}