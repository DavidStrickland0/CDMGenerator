using System.CommandLine;
using System.CommandLine.Invocation;
using CDMGenerator;
using System.Threading.Tasks;

// Define the options
var manifestOption = new Option<string>(
    name: "--manifest",
    description: "The path to the manifest file.")
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
    manifestOption,
    outputDirectoryOption
};

// Define the handler method
void ExecuteHandler(string manifest, string outputDirectory)
{
    var codeCreator = new DotNetSolutionWriter(); // Ensure your actual initialization logic here
    var modelGenerator = new ModelGenerator(p => codeCreator.ProcessFile(manifest ,p , outputDirectory));

    
    if (String.IsNullOrEmpty(outputDirectory) || Path.Exists(outputDirectory))
    {
        Console.WriteLine($"Loading {manifest}");

        // Example placeholder: Replace with your actual generation logic
        modelGenerator.Generate(manifest).Wait(); // Adjust based on the actual asynchronous handling in your application
    }
    else
    {
        Console.Write($"Output Directory {Path.GetFullPath(outputDirectory)} Does Not Exist.");
    }
    // Potentially use outputDirectory as needed
}

// Set the handler for the command
rootCommand.SetHandler((string manifest, string outputDirectory) =>
{
    ExecuteHandler(manifest, outputDirectory);
}, manifestOption, outputDirectoryOption);

// Execute the command
return await rootCommand.InvokeAsync(args);
