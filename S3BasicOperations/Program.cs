using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace S3BasicOperations
{
    class Program
    {
        private const string FirstBucketName = "firstbucketnamehere";
        private const string SecondBucketName = "secondbucketnamehere";
        private const string UploadDir = "upload";
        private const string DownloadDir = "download";
        private const string FileName1 = "file1.txt";
        private const string FileName2 = "file2.txt";

        private static readonly AmazonS3Client S3Client = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine($"Creating new bucket [{FirstBucketName}]:");
            await CreateBucket(FirstBucketName);
            Console.WriteLine($"Uploading files to [{FirstBucketName}]:");
            await UploadFile(FirstBucketName, FileName1);
            await UploadFile(FirstBucketName, FileName2);
            await ShowBucketContent(FirstBucketName);

            Console.WriteLine($"Generate download link for file {FileName1} from bucket [{FirstBucketName}]:");
            GenerateDownloadLink(FirstBucketName, FileName1, TimeSpan.FromSeconds(30));

            Console.WriteLine($"Downloading files from [{FirstBucketName}]:");
            await DownloadFile(FirstBucketName, FileName1, FileName1);
            await DownloadFile(FirstBucketName, FileName2, FileName2);

            Console.WriteLine($"Creating second bucket [{SecondBucketName}]:");
            await CreateBucket(SecondBucketName, true);

            Console.WriteLine($"Copying files from bucket [{FirstBucketName}] to second bucket [{SecondBucketName}]:");
            await CopyFile(FirstBucketName, SecondBucketName, FileName1, FileName1);
            await CopyFile(FirstBucketName, SecondBucketName, FileName2, FileName2);
            await ShowBucketContent(SecondBucketName);

            Console.WriteLine($"Deleting files from [{FirstBucketName}]:");
            await DeleteFiles(FirstBucketName, new[] { FileName1, FileName2 });
            await ShowBucketContent(FirstBucketName);

            Console.WriteLine($"Deleting files from [{SecondBucketName}]:");
            await DeleteFiles(SecondBucketName, new[] { FileName1, FileName2 });
            await ShowBucketContent(SecondBucketName);

            Console.WriteLine($"Deleting bucket [{FirstBucketName}]:");
            await DeleteBucket(FirstBucketName);
            Console.WriteLine($"Deleting bucket [{SecondBucketName}]:");
            await DeleteBucket(SecondBucketName);
        }

        private static async Task ShowBucketContent(string bucketName)
        {
            var bucket = await ListBucket(bucketName);
            if (bucket.S3Objects.Any())
            {
                Console.WriteLine($"Show bucket [{bucket.Name}] content:");
                foreach (var file in bucket.S3Objects)
                {
                    Console.WriteLine(file.Key);
                }
            }
            else
            {
                Console.WriteLine($"Bucket [{bucket.Name}] is empty");
            }
        }
        
        private static async Task<ListObjectsResponse> ListBucket(string bucketName)
        {
            try
            {
                return await S3Client.ListObjectsAsync(new ListObjectsRequest { BucketName = bucketName });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while listing bucket: {e.Message}");
            }

            return null;
        }

        private static async Task CreateBucket(string bucketName, bool secureBucket = default)
        {
            try
            {
                var response = await S3Client.PutBucketAsync(bucketName);
                if (response.HttpStatusCode == HttpStatusCode.OK && secureBucket)
                    await BlockPublicAccess(bucketName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while creating new bucket: {e.Message}");
            }
        }

        private static async Task DeleteBucket(string bucketName)
        {
            try
            {
                await S3Client.DeleteBucketAsync(bucketName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while deleting bucket: {e.Message}");
            }
        }
        
        private static async Task UploadFile(string bucketName, string fileName)
        {
            var filePath = Path.Join(Directory.GetCurrentDirectory(), UploadDir, fileName);
            try
            {
                await S3Client.PutObjectAsync(new PutObjectRequest() { FilePath = filePath, BucketName = bucketName });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while uploading file: {e.Message}");
            }
        }

        private static async Task DownloadFile(string bucketName, string localName, string keyName)
        {
            var filePath = Path.Join(Directory.GetCurrentDirectory(), DownloadDir, localName);
            try
            {
                var response = await S3Client.GetObjectAsync(new GetObjectRequest() { BucketName = bucketName, Key = keyName });
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    await using var responseStream = response.ResponseStream;
                    await response.WriteResponseStreamToFileAsync(filePath, false, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while downloading file: {e.Message}");
            }
        }

        private static async Task DeleteFiles(string bucketName, IEnumerable<string> keys)
        {
            var keyVer = keys.Select(k => new KeyVersion { Key = k }).ToList();
            try
            {
                await S3Client.DeleteObjectsAsync(new DeleteObjectsRequest() { BucketName = bucketName, Objects = keyVer });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while deleting file: {e.Message}");
            }
        }

        private static async Task CopyFile(string sourceBucket, string destBucket, string sourceKey, string destKey)
        {
            try
            {
                await S3Client.CopyObjectAsync(
                    new CopyObjectRequest
                    {
                        SourceBucket = sourceBucket,
                        DestinationBucket = destBucket,
                        SourceKey = sourceKey,
                        DestinationKey = destKey
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while copying file between buckets: {e.Message}");
            }
        }

        private static async Task BlockPublicAccess(string bucketName)
        {
            try
            {
                await S3Client.PutPublicAccessBlockAsync(new PutPublicAccessBlockRequest
                {
                    BucketName = bucketName,
                    PublicAccessBlockConfiguration = new PublicAccessBlockConfiguration
                    {
                        BlockPublicAcls = true,
                        IgnorePublicAcls = true,
                        BlockPublicPolicy = true,
                        RestrictPublicBuckets = true
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while blocking public access to bucket: {e.Message}");
            }
        }

        private static void GenerateDownloadLink(string bucketName, string key, TimeSpan expireTime)
        {
            try
            {
                var preSignedUrl = S3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    Expires = DateTime.Now.Add(expireTime)
                });
                Console.WriteLine($"Pre-signed url for bucket [{bucketName}] with key {key} is: {preSignedUrl}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while generating download url: {e.Message}");
            }
        }
    }
}