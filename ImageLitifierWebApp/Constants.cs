﻿namespace ImageLitifier.WebApp;

public class Constants
{
    public class EnvironmentVariables
    {
        public const string BlobStorageConnectionString = "BLOB_STORAGE_CONNECTION_STRING";
        public const string BlobStorageRequestsContainerName = "BLOB_STORAGE_REQUESTS_CONTAINER_NAME";
        public const string BlobStorageProcessedContainerName = "BLOB_STORAGE_PROCESSED_CONTAINER_NAME";
        public const string ServiceBusConnectionString = "SERVICE_BUS_CONNECTION_STRING";
        public const string ServiceBusQueueName = "SERVICE_BUS_QUEUE_NAME";
        public const string BackgroundMusicUrl = "BACKGROUND_MUSIC_URL";
    }
}
