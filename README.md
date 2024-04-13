# CDMGenerator

CDMGenerator is an open-source tool designed to generate .NET class libraries based on Microsoft's Common Data Model (CDM) schema. It enables .NET developers to integrate structured data models from CDM into their applications seamlessly.

## Features

- **Generate .NET Class Libraries**: Automatically creates .NET class libraries from CDM schema definitions.
- **Command Line Interface**: Simple CLI for easy integration and automation within your development workflow.
- **Support for Microsoft CDM**: Direct support for schemas defined by Microsoft's Common Data Model.

## Prerequisites

Before you begin, ensure you have the following installed:
- [.NET SDK](https://dotnet.microsoft.com/download) (version recommended by your application requirements)
- Git

## Installation

To get started with CDMGenerator, clone the repository and its submodule. Run these commands:

```bash
git clone https://github.com/DavidStrickland0/CDMGenerator.git
cd CDMGenerator
git submodule update --init --recursive
```

## Usage

To generate a class library from a CDM manifest file, use the following command format:

```bash
dotnet run --project CDMGenerator --schemaRoot <path-to-schema-documents> --manifestFile <path-to-manifest-file> --outputDirectory <output-directory>
```

### Example

Generate a library using the default manifest file:

```bash
dotnet run --project CDMGenerator --schemaRoot ./CDM/schemaDocuments/ --manifestFile CustomerInsightsJourneys/default.manifest.cdm.json --outputDirectory generatedOutput
```

This command generates a .NET library based on the `default.manifest.cdm.json` file, using the specified schema root as the root of the schema or local namespace in CDM terms. The output is saved to the specified directory.

## Contributing

Contributions are welcome! Feel free to submit pull requests, create issues, or propose new features.

1. **Fork** the repository on GitHub.
2. **Clone** the project to your machine.
3. **Commit** changes to your branch.
4. **Push** your work back up to your fork.
5. Submit a **Pull Request** so that we can review your changes

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Thanks to Microsoft for providing the Common Data Model which is pivotal to this project.

## Contact

For questions or feedback regarding CDMGenerator, please file an issue on our GitHub repository.