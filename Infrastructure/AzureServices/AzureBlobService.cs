using Application.Interfaces.IInfrastructure.IAzureServices;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Domain.Models.Azure;

namespace Infrastructure.AzureServices
{
    public class AzureBlobService : IAzureBlobService
    {
        public string ConnectionString { get; set; }
        private string Description { get; set; }

        public AzureBlobService(AzureStorageConnectionModel azureStorageConnectionModel)
        {
            ConnectionString = azureStorageConnectionModel.ConnectionString;
            Description = azureStorageConnectionModel.Description;
            Task.Run(SetCorsRulesAsync).ConfigureAwait(false);
        }

        public async Task<bool> BlobExistAsync(string containerName, string fileName)
        {
            BlobContainerClient container = await CreateContainerIfNotExistsAsync(containerName);

            BlobClient blob = container.GetBlobClient(fileName);

            return blob.Exists();
        }

        public async Task<bool> DeleteBlob(string containerName, string fileName)
        {
            BlobContainerClient container = await CreateContainerIfNotExistsAsync(containerName);

            BlobClient blob = container.GetBlobClient(fileName);

            return await blob.DeleteIfExistsAsync();
        }

        public void DeleteContainerIfExists(string containerName)
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> DownLoadAsByteArrayAsync(string containerName, string fileName)
        {
            BlobContainerClient container = await CreateContainerIfNotExistsAsync(containerName);

            if (await container.ExistsAsync())
            {
                BlobClient blob = container.GetBlobClient(fileName);

                if (await blob.ExistsAsync())
                {
                    BlobDownloadInfo download = await blob.DownloadAsync();

                    await using var stream = new MemoryStream();
                    await download.Content.CopyToAsync(stream);
                    stream.Position = 0;
                    return stream.ToArray();
                }
                Console.WriteLine("Blob name not found.");
                return null;
            }

            Console.WriteLine("Container not found.");
            return null;
        }

        public Task<string> GetBlobPath(string containerName, string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetBlobUriIfExists(string containerName, string fileName)
        {
            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = await CreateContainerIfNotExistsAsync(containerName);

            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient(fileName);

            return blob.Exists() ? blob.Uri.ToString() : string.Empty;
        }

        public async Task<Uri> GetServiceSasUriForBlob(string containerName, string fileName, string storedPolicyName = null, int hours = 20)
        {
            BlobContainerClient container = await CreateContainerIfNotExistsAsync(containerName);

            BlobClient blobClient = container.GetBlobClient(fileName);
            if (!blobClient.Exists()) { return null; }

            if (blobClient.CanGenerateSasUri)
            {
                return GetSasUri(blobClient, storedPolicyName, hours);
            }
            else
            {
                Console.WriteLine(@"BlobClient must be authorized with Shared Key 
                  credentials to create a service SAS.");
                return null;
            }
        }

        public async Task<string> Upload(MemoryStream stream, string containerName, string fileName, IDictionary<string, string> tags = null)
        {
            var options = new BlobUploadOptions();
            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = await CreateContainerIfNotExistsAsync(containerName);

            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient(fileName);
            stream.Position = 0;
            // Upload local file
            await blob.UploadAsync(stream, true);

            if (tags is not null)
               await blob.SetTagsAsync(tags);

            return blob.Uri.ToString();
        }
        public async Task<BlobContainerClient> CreateContainerIfNotExistsAsync(string containerName)
        {
            try
            {
                var serviceClient = new BlobServiceClient(ConnectionString);
                BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(containerName);
                if (!await containerClient.ExistsAsync())
                {
                    containerClient = await serviceClient.CreateBlobContainerAsync(containerName);
                }

                return containerClient;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine(ex.Message);
                return new BlobContainerClient(ConnectionString, containerName);
            }
        }

        public async Task SetCorsRulesAsync()
        {
            var rules = new BlobCorsRule
            {
                AllowedMethods = "PUT,GET,POST",
                AllowedOrigins = "*",
                ExposedHeaders = "*",
                AllowedHeaders = "*",
                MaxAgeInSeconds = 180
            };
            var properties = new BlobServiceProperties
            {
                Cors = new List<BlobCorsRule>
                {
                    rules
                }
            };

            var serviceClient = new BlobServiceClient(ConnectionString);
            await serviceClient.SetPropertiesAsync(properties);
        }
        private Uri GetSasUri(BlobClient blobClient, string storedPolicyName = null, int hours = 24)
        {
            // Create a SAS token that's valid for one hour.
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            if (storedPolicyName == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(hours);
                sasBuilder.SetPermissions(BlobSasPermissions.All);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            Console.WriteLine("SAS URI for blob is: {0}", sasUri);
            Console.WriteLine();

            return sasUri;
        }
    }
}
