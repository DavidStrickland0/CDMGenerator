using System.CommandLine;
using System.CommandLine.Invocation;
using CDMGenerator;
using System.Threading.Tasks;

// Define the options
var rootManifestPath = new Option<string>(
    name: "--manifestFile",
    description: "The path to the manifest file.")
{
    IsRequired = true // Marking the manifest option as required
};

// Define the options
var schemaRootDirectory = new Option<string>(
    name: "--schemaRoot",
    description: "The root Directory of all schema files.")
{
    IsRequired = true // Marking the manifest option as required
};

var outputDirectoryOption = new Option<string>(
    name: "--outputDirectory",
    description: "The output directory where the generated files will be saved.",
    getDefaultValue: () => "./"); // Providing a default value for the output directory

// Create the command
var rootCommand = new RootCommand("Application that generates code based on a given manifest.")
{
    schemaRootDirectory,
    rootManifestPath,
    outputDirectoryOption
};

// Define the handler method
void ExecuteHandler(string schemaRoot,string manifest, string outputDirectory)
{
    var codeCreator = new DotNetSolutionWriter(); // Ensure your actual initialization logic here
    var modelGenerator = new ModelGenerator(p => codeCreator.ProcessFile(manifest ,p , outputDirectory));

    
    if (String.IsNullOrEmpty(outputDirectory) || Path.Exists(outputDirectory))
    {
        // Example placeholder: Replace with your actual generation logic
        modelGenerator.Generate(schemaRoot, manifest).Wait(); // Adjust based on the actual asynchronous handling in your application
    }
    else
    {
        Console.Write($"Output Directory {Path.GetFullPath(outputDirectory)} Does Not Exist.");
    }
    // Potentially use outputDirectory as needed
}

// Set the handler for the command
rootCommand.SetHandler(ExecuteHandler,schemaRootDirectory, rootManifestPath, outputDirectoryOption);

// Execute the command
return await rootCommand.InvokeAsync(args);
