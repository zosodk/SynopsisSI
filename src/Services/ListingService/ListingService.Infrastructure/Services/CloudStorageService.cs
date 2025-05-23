    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using SynopsisSI.Services.ListingService.Application.Interfaces.Infrastructure;
    using System;

    namespace SynopsisSI.Services.ListingService.Infrastructure.Services;

    public class CloudStorageService : ICloudStorageService
    {
        private readonly string _baseStorageUrl;
        private readonly ILogger<CloudStorageService> _logger;

        public CloudStorageService(IConfiguration configuration, ILogger<CloudStorageService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var s3BucketName = configuration["CloudStorage:S3BucketName"];
            var awsRegion = configuration["CloudStorage:Region"];
            var serviceUrl = configuration["CloudStorage:ServiceURL"];

            if (!string.IsNullOrEmpty(serviceUrl))
            {
                if (string.IsNullOrWhiteSpace(s3BucketName))
                {
                    _logger.LogError("CloudStorage:S3BucketName is required when CloudStorage:ServiceURL is provided.");
                    throw new InvalidOperationException("S3BucketName configuration is missing for custom service URL.");
                }
                _baseStorageUrl = $"{serviceUrl.TrimEnd('/')}/{s3BucketName.Trim()}";
            }
            else if (!string.IsNullOrEmpty(s3BucketName) && !string.IsNullOrEmpty(awsRegion))
            {
                _baseStorageUrl = $"https://{s3BucketName.Trim()}.s3.{awsRegion.Trim()}.amazonaws.com";
            }
            else
            {
                _logger.LogError("Cloud storage base URL could not be determined. Required configuration is missing under 'CloudStorage'.");
                throw new InvalidOperationException("Cloud storage URL configuration is incomplete or invalid.");
            }
            _logger.LogInformation("CloudStorageService initialized with base URL: {BaseStorageUrl}", _baseStorageUrl);
        }

        public string GetPublicUrl(string objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                _logger.LogWarning("GetPublicUrl called with empty or whitespace objectKey.");
                throw new ArgumentException("Object key cannot be empty or whitespace.", nameof(objectKey));
            }
            return $"{_baseStorageUrl.TrimEnd('/')}/{objectKey.TrimStart('/')}";
        }
    }