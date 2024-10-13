# DotSmog

**DotSmog** is a distributed .NET system designed for real-time smog monitoring. This application leverages modern containerization technologies to provide a scalable and efficient solution for tracking air quality.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Features

- Real-time monitoring of smog levels.
- Scalable architecture using Docker.
- Easy to set up and run locally or in a containerized environment.

## Requirements

- **Docker**: A platform for developing, shipping, and running applications in containers.
- **Docker Compose**: A tool for defining and running multi-container Docker applications.
- **.NET 8.0**: The latest version of the .NET framework for building applications.

## Installation

### Clone the Repository

```bash
git clone https://github.com/Wleszcz/DotSmog.git
cd dotsmog
```

## Usage

### Running with Docker

To run the DotSmog application using Docker, follow these steps:

1. **Build the Docker Image**  
   Run the following command to build the Docker image for the application:  
   ```bash
   docker-compose build
   ```

2. **Start the Application**  
   Use the following command to start the application:  
   ```bash
   docker-compose up
   ```  
   This will launch the application in the background, and you will see the logs in your terminal.

3. **Access the Application**  
   After the application is running, you can access it via your web browser. By default, the application runs on `http://localhost:5275`. Check the logs for any specific URL if it differs.
   ```bash
   curl http://localhost:5275/weatherforecast/
   ```

### Running Locally

To run DotSmog locally without Docker, follow these steps:

1. **Ensure .NET 8.0 is Installed**  
   Make sure you have .NET 8.0 installed on your system. You can download it from the [.NET website](https://dotnet.microsoft.com/download).

2. **Run the Application**  
   Use the following command to run the application locally:  
   ```bash
   dotnet run
   ```  
   Similar to the Docker setup, you can access the application via `http://localhost:5275`.

## Contributing

Contributions are welcome! If you have suggestions for improvements or features, please open an issue or submit a pull request.

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes and commit them (`git commit -m 'Add a feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
