using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using qei.Models;

namespace qei.Services
{
    public class QeiService : IQeiService
    {
        private const string blobName = "qei.json";
        private readonly IConfiguration Configuration;

        public QeiService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<bool> Add(string database, string key, string value)
        {
            var connectionString = Configuration["AZURE_STORAGE_CONNECTION_STRING"];
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(database);

            var blobClient = containerClient.GetBlobClient(blobName);
            if (blobClient.Exists())
            {
                var stream = await blobClient.OpenReadAsync();

                var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();
                var qeis = JsonConvert.DeserializeObject<List<qeiModel>>(text);

                qeis.Add(new qeiModel() { Key = key, Value = value });

                var qeisSerialized = JsonConvert.SerializeObject(qeis);

                var memoryStream = GenerateStreamFromString(qeisSerialized);
                await blobClient.DeleteIfExistsAsync();
                await blobClient.UploadAsync(memoryStream);
            }
            else
            {
                var qeis = new List<qeiModel>();
                qeis.Add(new qeiModel() { Key = key, Value = value });

                var qeisSerialized = JsonConvert.SerializeObject(qeis);

                MemoryStream memoryStream = GenerateStreamFromString(qeisSerialized);
                await blobClient.UploadAsync(memoryStream);
            }

            return true;
        }

        public async Task<string> Get(string database, string key)
        {
            var connectionString = Configuration["AZURE_STORAGE_CONNECTION_STRING"];
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(database);

            var blobClient = containerClient.GetBlobClient(blobName);
            var stream = await blobClient.OpenReadAsync();

            var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var qeis = JsonConvert.DeserializeObject<List<qeiModel>>(text);

            var qei = qeis.FirstOrDefault(c => c.Key == key);

            if (qei == null)
                throw new Exception("Key doesn't exist");

            return qei.Value;
        }

        public async Task<string> Create(string email)
        {
            var connectionString = Configuration["AZURE_STORAGE_CONNECTION_STRING"];
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerName = CreateMD5(email);
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            container.CreateIfNotExists();
            return containerName;
        }

        private string CreateMD5(string input)
        {
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }

        private MemoryStream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
