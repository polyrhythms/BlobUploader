using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlobUploader
{
    public class BlobService
    {
        private readonly BlobClientOptions ClientOptions;
        private readonly BlobUploadOptions UploadOptions;
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

            ServiceClient = new BlobServiceClient(connectionString, ClientOptions);
            Log = log;
        }

        public async Task<MemoryStream> ReadFile(
            string shareName, string pathName, string fileName)
        {
            var containerClient = ServiceClient.GetBlobContainerClient(shareName);
            var blobClient = containerClient.GetBlobClient(pathName + fileName);

            var readStream = new MemoryStream();
            await blobClient.DownloadToAsync(readStream);

            return readStream;
        }

        public async Task WriteFile(string localPath, string containerName, string blobPath,
            bool overwrite)
        {
            try
            {
                var containerClient = ServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobPath);

                if (overwrite)
                {
                    await blobClient.DeleteIfExistsAsync();
                }
                else
                {
                    var exists = await blobClient.ExistsAsync();
                    if (exists.Value)
                    {
                        return;
                    }
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
