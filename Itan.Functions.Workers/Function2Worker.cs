using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using Itan.Functions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace Itan.Functions.Workers
{
    public class Function2Worker
    {
        private readonly ILogger log;
        private readonly string functionAppDirectory;

        public Function2Worker(
            ILogger log,
            string functionAppDirectory)
        {
            this.log = log;
            this.functionAppDirectory = functionAppDirectory;
        }

        public async Task Run(string myQueueItem)
        {
            var channelToDownload = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelToDownload>(myQueueItem);
            var client = HttpClientFactory.Create();
            var channelString = string.Empty;
            try
            {
                channelString = await client.GetStringAsync(channelToDownload.Url);
            }
            catch (Exception e)
            {
                this.log.LogWarning(e.ToString());
                this.log.LogWarning($"Ending {nameof(Function2Worker)} in exception handler for `await client.GetStringAsync(channelToDownload.Url);`");
                return;
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(this.functionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var emulatorConnectionString = config.GetConnectionString("emulator");


            var hashCode = channelString.GetHashCode();
            var sqlConnectionStringReader = config.GetConnectionString("sql-itan-reader");

            var checkForExistenceQuery = "SELECT * FROM ChannelDownloads WHERE ChannelId = @channelId AND HashCode = @hashCode";
            var checkForExistenceQueryData = new { channelId = channelToDownload.Id, hashCode = hashCode };

            using (var sqlConnection = new SqlConnection(sqlConnectionStringReader))
            {
                var result = await sqlConnection.QuerySingleOrDefaultAsync(checkForExistenceQuery, checkForExistenceQueryData);
                if (result != null)
                {
                    return;
                }
            }

            var account = CloudStorageAccount.Parse(emulatorConnectionString);
            var serviceClient = account.CreateCloudBlobClient();
            var container = serviceClient.GetContainerReference("rss");
            await container.CreateIfNotExistsAsync();
            var channelDownloadPath = $"raw/{channelToDownload.Id}/{DateTime.UtcNow.ToString("yyyyMMddhhmmss_mmm")}.xml";
            var blob = container.GetBlockBlobReference(channelDownloadPath);
            await blob.UploadTextAsync(channelString);

            var query = "INSERT INTO ChannelDownloads (Id, ChannelId, Path, CreatedOn, HashCode) VALUES (@id, @channelId, @path, @createdOn, @hashCode)";

            var data = new
            {
                id = Guid.NewGuid(),
                channelId = channelToDownload.Id,
                path = channelDownloadPath,
                createdOn = DateTime.UtcNow,
                hashCode = hashCode
            };

            var sqlConnectionString = config.GetConnectionString("sql-itan-writer");

            try
            {
                using (var sqlConnection = new SqlConnection(sqlConnectionString))
                {
                    await sqlConnection.ExecuteAsync(query, data);
                }
            }
            catch (Exception e)
            {
                this.log.LogCritical(e.ToString());
                throw;
            }
        }
    }
}