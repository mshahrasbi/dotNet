using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

namespace WebAdvert.Web.Services
{
    public class S3FileUploader : IFileUploader
    {
        private readonly IConfiguration _configuration;
        public S3FileUploader(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public async Task<bool> UploadFileAsync(string filename, Stream storageStream)
        {
            // this is for small files. For bigger file size we have to do differently.
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("File name muse be specified");
            }

            var bucketname = this._configuration.GetValue<string>("ImageBucket");

            using (var client = new AmazonS3Client())
            {
                if (storageStream.Length > 0)
                {
                    if (storageStream.CanSeek)
                    {
                        storageStream.Seek(0, SeekOrigin.Begin);
                    }
                }

                var request = new PutObjectRequest
                {
                    AutoCloseStream = true,
                    BucketName = bucketname,
                    InputStream = storageStream,
                    Key = filename
                };

                var response = await client.PutObjectAsync(request).ConfigureAwait(false);

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
        }
    }
}
