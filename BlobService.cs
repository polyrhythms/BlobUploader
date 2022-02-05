using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlobUploader
{
    public class BlobService
    {
        private readonly BlobClientOptions ClientOptions;
        private readonly BlobUploadOptions UploadOptions;
        private readonly CloudStorageAccount StorageAccount;
        private readonly BlobServiceClient ServiceClient;
        private readonly StreamWriter Log;

        public BlobService(string connectionString, StreamWriter log)
        {
            ClientOptions = new BlobClientOptions();
            ClientOptions.Retry.MaxRetries = 5;
            UploadOptions = new BlobUploadOptions
            {
                TransferOptions = new StorageTransferOptions
                {
                    MaximumTransferSize = 4 * 1024 * 1024,
                    InitialTransferSize = 4 * 1024 * 1024
                }
            };

            StorageAccount = CloudStorageAccount.Parse(connectionString);
            ServiceClient = new BlobServiceClient(connectionString, ClientOptions);
            Log = log;
        }

        public async Task<MemoryStream> ReadFile(
            string shareName, string pathName, string fileName)
        {
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(shareName);
            var cloudBlockBlob = container.GetBlockBlobReference(pathName + fileName);

            var readStream = new MemoryStream();
            await cloudBlockBlob.DownloadToStreamAsync(readStream);
            return readStream;
        }

        public async Task WriteFile(string localPath, string containerName, string blobPath)
        {
            try
            {
                var containerClient = ServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobPath);

                // TODO: add a flag to either delete if it exists or skip if it exists.
                // await blobClient.DeleteIfExistsAsync();
                var exists = await blobClient.ExistsAsync();
                if (exists.Value)
                {
                    return;
                }

                Console.WriteLine("Writing " + blobPath);
                await blobClient.UploadAsync(path: localPath, options: UploadOptions);
                Log.WriteLine("Finished " + blobPath);
                Log.Flush();
            }
            catch (Exception e)
            {
                Log.WriteLine("****** START OF ERROR ****** ");
                Log.WriteLine(blobPath + " failed: " + e.Message);
                Log.WriteLine("******* END OF ERROR ******* ");
                Log.Flush();
            }
        }
    }
}
