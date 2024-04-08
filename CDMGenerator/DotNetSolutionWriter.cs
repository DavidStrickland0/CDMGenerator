// See https://aka.ms/new-console-template for more information
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Reflection;
using System.Runtime.Versioning;

internal class DotNetSolutionWriter
{
    private bool finalized = false;
    private bool initialized = false;
    string projectName=String.Empty;

    ~DotNetSolutionWriter()
    {
        finalize();
    }
    public DotNetSolutionWriter()
    {
    }
    public void ProcessFile(string manifest, string classCode, string directory)
    {

        var outputDirectory = String.IsNullOrEmpty(directory) ? Path.GetTempPath() : directory;
        if (!initialized)
        {
            initialize(manifest, outputDirectory);
        }


        var syntaxTree = CSharpSyntaxTree.ParseText(classCode);
        var root = syntaxTree.GetRoot();

        // Find the first class declaration in the code
        var classDeclaration = root.DescendantNodes()
                                    .OfType<ClassDeclarationSyntax>()
                                    .FirstOrDefault();

        // Continue only if a class declaration is found
        if (classDeclaration != null)
        {
            // Get the namespace if any
            var namespaceDeclaration = classDeclaration.Ancestors()
                                                       .OfType<NamespaceDeclarationSyntax>()
                                                       .FirstOrDefault();

            var namespaceName = namespaceDeclaration?.Name.ToString() ?? string.Empty;
            var className = classDeclaration.Identifier.ValueText;

            // Fully qualified name
            var fullyQualifiedName = string.IsNullOrEmpty(namespaceName) ? className : $"{namespaceName}.{className}";

            // Convert namespace to directory path
            var directoryPath = Path.Combine(outputDirectory,projectName, namespaceName.Replace(".", Path.DirectorySeparatorChar.ToString()));
            var filePath = Path.Combine(directoryPath, $"{className}.cs");

            // Ensure the directory exists
            Directory.CreateDirectory(directoryPath);

            // Save the class code to the file
            File.WriteAllText(filePath, classCode);

            Console.WriteLine($"Saved {fullyQualifiedName} to {filePath}");
        }
    }
    private void finalize()
    {
        if (initialized && !finalized)
        {
            finalized = true;
        }
    }
    private void initialize(string manifest, string outputDirectory)
    {
        if (!initialized)
        {
            initialized = true;
            createProjectFile(manifest, outputDirectory);
        }
    }
    private void createProjectFile(string manifestPath, string outputDirectory)
    {
        var filename = Path.GetFileName(manifestPath);
        const string suffixToRemove = ".manifest.cdm.json";
        if (filename.EndsWith(suffixToRemove))
        {
            // Remove the specified suffix
            filename = filename.Substring(0, filename.Length - suffixToRemove.Length);

            // Split into parts by '.'
            var parts = filename.Split('.');
            string version = "1.0.0"; // Default version if no numeric parts are found
            projectName = filename;

            // Traverse the parts from the end to find version numbers
            for (int index = parts.Length - 1; index >= 0; index--)
            {
                // Check if part is a semantic version number
                if (Regex.IsMatch(parts[index], @"^\d+\.\d+\.\d+$"))
                {
                    version = parts[index];
                    // Assume everything before the version part is the project name
                    projectName = string.Join(".", parts.Take(index));
                    break;
                }
            }

            // Generate the csproj file content
            var csProjContent = generateCsProjContent(projectName, version);

            // Create the csproj Dir
            var csProjDirPath = Path.Combine(outputDirectory, projectName);
            if(!Directory.Exists(csProjDirPath))
            {
                Directory.CreateDirectory(csProjDirPath);
            }

            // Define the path for the new csproj file
            var csProjFilePath = Path.Combine(csProjDirPath, $"{projectName}.csproj");

            // Write the csproj content to file
            File.WriteAllText(csProjFilePath, csProjContent);
        }
        else
        {
            throw new ArgumentException("The provided manifest filename does not match the expected pattern.");
        }
    }

    private string generateCsProjContent(string projectName, string version)
    {
        // Attempt to get the target framework of the currently executing assembly
        var targetFrameworkAttribute = Assembly.GetExecutingAssembly()
            .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
            .FirstOrDefault() as TargetFrameworkAttribute;

        string targetFramework = targetFrameworkAttribute?.FrameworkName ?? "net6.0"; // Default if not found

        // Simplify the framework name to a format commonly used in csproj files (e.g., net6.0, netcoreapp3.1)
        // This part may need adjustments based on specific needs and conventions
        var frameworkMoniker = SimplifyFrameworkName(targetFramework);

        XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
        var project = new XElement(ns + "Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
            new XElement(ns + "PropertyGroup",
                new XElement(ns + "OutputType", "Library"),
                new XElement(ns + "TargetFramework", frameworkMoniker),
                new XElement(ns + "RootNamespace", projectName),
                new XElement(ns + "AssemblyName", projectName),
                new XElement(ns + "Version", version)
            )
        );

        return project.ToString();
    }

    // Simplifies the .NET framework name to a more standard TFM (Target Framework Moniker)
    // Adjust the implementation as necessary to fit the framework naming conventions you're targeting
    private string SimplifyFrameworkName(string frameworkName)
    {
        if (frameworkName.Contains(".NETCoreApp", StringComparison.OrdinalIgnoreCase))
        {
            // Example: .NETCoreApp,Version=v3.1 => netcoreapp3.1
            var version = frameworkName.Split('=')[1].Trim('v');
            return $"netcoreapp{version}";
        }
        else if (frameworkName.Contains(".NETFramework", StringComparison.OrdinalIgnoreCase))
        {
            // Handle .NET Framework
        }
        else if (frameworkName.Contains(".NETStandard", StringComparison.OrdinalIgnoreCase))
        {
            // Handle .NET Standard
        }
        else if (frameworkName.StartsWith(".NET", StringComparison.OrdinalIgnoreCase))
        {
            // For .NET 5 and above, where the attribute might simply be ".NET,Version=5.0"
            var version = frameworkName.Split('=')[1];
            return $"net{version}";
        }

        // Default or unrecognized framework; adjust as necessary
        return "net6.0";
    }
}
