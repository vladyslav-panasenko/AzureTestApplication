using Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace UploadSecretToBlob
{
    public class UploadSecretToBlob
    {
        static readonly HttpClient Client = new HttpClient();

        [FunctionName("UploadSecretToBlob")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var controller = "keyvault";

            var apiUrl = Environment.GetEnvironmentVariable(Constants.ApiUrlName);
            log.LogInformation(apiUrl);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}{controller}");
                requestMessage.Headers.Add(Constants.SubscriptionKeyHeader, Environment.GetEnvironmentVariable(Constants.SubscriptionKeyName));

                var response = await Client.SendAsync(requestMessage);

                log.LogInformation(response.StatusCode.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var streamResult = await response.Content.ReadAsStreamAsync();

                    await this.WriteToBlob(streamResult, log);
                }

                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }

        private async Task WriteToBlob(Stream source, ILogger log)
        {
            if (CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable(Constants.StorageAccountConnectionStringName), out CloudStorageAccount cloudStorageAccount))
            {
                var fileName = DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm:ss");

                var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference(Constants.ContainerName);
                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference($"{fileName}.json");

                await cloudBlockBlob.UploadFromStreamAsync(source);
            }
            else
            {
                log.LogInformation($"Cannot find storage connection string: {DateTime.Now}");
            }
        }
    }
}
