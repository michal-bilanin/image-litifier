namespace ImageLitifier.ImageProcessor;

public class Constants
{
    public class EnvironmentVariables
    {
        public const string BlobStorageConnectionString = "BLOB_STORAGE_CONNECTION_STRING";
        public const string BlobStorageProcessedContainerName = "BLOB_STORAGE_PROCESSED_CONTAINER_NAME";
        public const string BlobStorageRequestsContainerName = "BLOB_STORAGE_REQUESTS_CONTAINER_NAME";
        public const string ServiceBusConnectionString = "SERVICE_BUS_CONNECTION_STRING";
        public const string ServiceBusQueueName = "%SERVICE_BUS_QUEUE_NAME%";
    }
}
