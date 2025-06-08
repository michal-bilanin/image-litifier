# ImageLitifier

ImageLitifier is a cloud-based image processing application that processes images by overlaying a flame GIF effect. It uses Azure Service Bus for message queuing, Azure Blob Storage for file management, and the `ImageSharp` library for image manipulation.

## Features

- Processes images by applying a flame GIF effect.
- Uses Azure Blob Storage for storing original and processed images.
- Integrates with Azure Service Bus for message-driven processing.
- Handles various image formats and provides error handling for invalid or corrupted images.

## Technologies Used

- **C#** (.NET 6)
- **Azure Service Bus** for message queuing.
- **Azure Blob Storage** for file storage.
- **ImageSharp** for image processing.
- **Dependency Injection** via `Microsoft.Extensions.DependencyInjection`.
- **Logging** via `Microsoft.Extensions.Logging`.

## Project Structure

- `ImageLitifier.ImageProcessor`: Contains the main image processing logic.
- `Services`: Includes the `LitnessService` for applying the flame GIF effect.
- `Constants.cs`: Defines environment variable keys and other constants.
- `launchSettings.json`: Configures environment variables for local development.

## Prerequisites

- .NET 6 SDK
- Azure Service Bus instance
- Azure Blob Storage account
- A flame GIF file uploaded to Azure Blob Storage

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
