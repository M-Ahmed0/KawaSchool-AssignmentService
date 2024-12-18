using Azure;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IInfrastructure.IAzureServices
{
    public interface IAzureBlobService
    {
        Task<bool> BlobExistAsync(string containerName, string fileName);
        Task<bool> DeleteBlob(string containerName, string fileName);
        void DeleteContainerIfExists(string containerName);
        Task<byte[]> DownLoadAsByteArrayAsync(string containerName, string fileName);
        Task<string> GetBlobPath(string containerName, string fileName);
        Task<string> GetBlobUriIfExists(string containerName, string fileName);
        Task<Uri> GetServiceSasUriForBlob(string containerName, string fileName, string storedPolicyName = null, int minutes = 20);
        Task<string> Upload(MemoryStream stream, string containerName, string fileName, IDictionary<string, string> tags = null);
        Task SetCorsRulesAsync();
        Task<BlobContainerClient> CreateContainerIfNotExistsAsync(string containerName);
    }
}
