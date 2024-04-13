# CDMGenerator

CDMGenerator is an open-source tool specifically crafted to generate .NET class libraries from schemas based on Microsoft's Common Data Model (CDM). This powerful utility enables .NET developers to effortlessly incorporate structured data models governed by the CDM into their applications, thus enhancing data interoperability and consistency across diverse systems.

## What is the Common Data Model (CDM)?

The Common Data Model (CDM) is a standardized, extensible data model provided by Microsoft that facilitates data interoperability between applications and services. By adhering to CDM, organizations can ensure their data assets are more comprehensible and usable across various platforms, promoting a unified data environment. The CDM encompasses a broad range of industry-standard definitions of data entities, attributes, and relationships, serving as a foundational schema upon which developers can build detailed, specific data models.

## Features

- **Generate .NET Class Libraries**: CDMGenerator automatically transforms CDM schema definitions into .NET class libraries, streamlining the development process and reducing manual coding errors.
- **Command Line Interface**: Utilizes a straightforward CLI to seamlessly integrate and automate within your development workflow, making it highly accessible for continuous integration environments.
- **Support for Microsoft CDM**: Fully supports schemas defined within Microsoft's Common Data Model, ensuring compatibility and ease of integration with other CDM-compliant tools and systems.

## Prerequisites

Before installation, make sure the following are installed:
- [.NET SDK](https://dotnet.microsoft.com/download)
- Git

## Installation

Start by cloning the repository and initializing its submodule:

```bash
git clone https://github.com/DavidStrickland0/CDMGenerator.git
cd CDMGenerator
git submodule update --init --recursive
```

## Usage

Generate a class library from a CDM manifest file with this command:

```bash
dotnet run --project CDMGenerator --schemaRoot <path-to-schema-documents> --manifestFile <path-to-manifest-file> --outputDirectory <output-directory>
```

### Example

For instance, to generate a library using a default manifest file:

```bash
dotnet run --project CDMGenerator --schemaRoot ./CDM/schemaDocuments/ --manifestFile CustomerInsightsJourneys/default.manifest.cdm.json --outputDirectory generatedOutput
```

This command will produce a .NET library based on the `default.manifest.cdm.json`, leveraging the specified schema documents. The resulting output will be stored in the designated directory.

## Contributing

We encourage contributions! To get involved:
1. **Fork** the repository on GitHub.
2. **Clone** the project to your machine.
3. **Commit** changes to your branch.
4. **Push** your work back up to your fork.
5. Submit a **Pull Request** so we can review your changes.

## License

Licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Special thanks to Microsoft for developing the Common Data Model, which plays a crucial role in this project.

## Contact

For questions or feedback about CDMGenerator, please open an issue on our GitHub repository.

This expanded README provides a more detailed explanation of the Common Data Model and clearly outlines how the CDMGenerator tool enhances the development process by leveraging the standardization offered by CDM.
