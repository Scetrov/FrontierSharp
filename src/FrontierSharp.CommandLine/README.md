
# FrontierSharp.CommandLine

## Description

`frontierctl` is a cross-platform command line application designed for interacting with the EVE Frontier world via the API, Blockchain and Mud Tables.

## Downloading Binaries

You can download binaries from the [Releases](https://github.com/Scetrov/FrontierSharp/releases) page.

## Running with Docker

You can run `frontierctl` using Docker. The following command will run the latest version of `frontierctl`:

```sh
docker run ghcr.io/scetrov/frontiersharp:latest rider --name Scetrov
```

## Building from Source

To build `frontierctl` from source, you can use the following steps:

1. Clone the repository:
    ```sh
    git clone https://github.com/scetrov/FrontierSharp.git
    ```

2. Navigate to the project directory:
    ```sh
    cd FrontierSharp.CommandLine
    ```

3. Install the required dependencies:
    ```sh
    dotnet restore
    ```

4. Build the project:
    ```sh
    dotnet build --configuration Release
    ```

### Usage

After installation, you can use the CLI tool by finding `frontierctl` in the `bin/Release/net9.0` directory of the project. You can run it directly from the command line:

```sh
frontierctl --help
```

## Contributing

We welcome contributions to FrontierSharp.CommandLine! If you would like to contribute, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Make your changes and commit them with clear and concise messages.
4. Push your changes to your fork.
5. Create a pull request to the main repository.

Please ensure that your code adheres to the project's coding standards and includes appropriate tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
