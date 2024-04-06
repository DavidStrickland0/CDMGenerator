namespace CDMGenerator
{
    using Microsoft.CommonDataModel.ObjectModel.Cdm;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;
    using System.Linq;

    public class CdmToPocoGenerator
    {
        public static string GeneratePocoClass(CdmEntityDefinition cdmEntity, CdmManifestDefinition manifest)
        {
            // Generate property declarations from CdmTypeAttributeDefinitions
            var properties = cdmEntity.Attributes
                .Where(attr => attr is CdmTypeAttributeDefinition)
                .Cast<CdmTypeAttributeDefinition>()
                .Select(attr => GenerateProperty(attr))
                .ToArray();

            // Prepare the XML comment based on the entity's description
            string comment = $"/// <summary>\n/// {cdmEntity.Description ?? "No description available."}\n/// </summary>\n";

            // Create the class declaration with the comment as leading trivia
            var classDeclaration = SyntaxFactory.ClassDeclaration(cdmEntity.EntityName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(properties)
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(comment));

            // Create the namespace declaration
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(manifest.ManifestName))
                .AddMembers(classDeclaration);

            // Create the compilation unit (the complete syntax tree)
            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();

            return syntaxTree.ToFullString();
        }


        private static MemberDeclarationSyntax GenerateProperty(CdmTypeAttributeDefinition attr)
        {
            // Determine the C# type for the CDM attribute
            string cSharpType = MapCdmTypeToCSharpType(attr); // Assuming a method that maps CDM data formats to C# types

            // Check if attribute name is a C# reserved keyword and prepend with '@' if necessary
            string propertyName = IsCSharpKeyword(attr.Name) ? "@" + attr.Name : attr.Name;

            // Prepare the XML comment based on the attribute's description
            string comment = $"/// <summary>\n/// {attr.Description ?? "No description available."}\n/// </summary>\n";

            // Create the property with the comment as leading trivia
            var property = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(cSharpType), propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(comment));

            return property;
        }
        private static bool IsCSharpKeyword(string name)
        {
            // Simplified check, consider using a more complete list of C# keywords
            return name switch
            {
                "class" => true,
                "int" => true,
                "string" => true,
                "namespace" => true,
                // Add other keywords as necessary
                _ => false,
            };
        }
        private static string MapCdmTypeToCSharpType(CdmTypeAttributeDefinition attr)
        {
            // This function should map the CDM data types to the corresponding C# data types
            // This example only handles a few cases for demonstration purposes
            if (attr.DataType != null)
            {
                switch (attr.DataType.NamedReference?.ToLower())
                {
                    case "string":
                        return "string";
                    case "int16":
                    case "int32":
                        return "int";
                    case "int64":
                        return "long";
                    case "boolean":
                        return "bool";
                    case "datetimeoffset":
                        return "DateTimeOffset";
                    // Add more cases as necessary
                    default:
                        return "object"; // Fallback for unmapped types
                }
            }
            else
            {
                switch (attr.DataFormat)
                {
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.Boolean:
                        return nameof(System.Boolean);
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.DateTimeOffset:
                        return nameof(System.DateTimeOffset);
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.Decimal:
                        return nameof(System.Decimal);
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.Double:
                        return nameof(System.Double);
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.Guid:
                        return nameof(System.Guid);
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.Int16:
                        return nameof(System.Int16);
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.Int32:
                        return nameof(System.Int32);
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.Int64:
                        return nameof(System.Int64);
                    case Microsoft.CommonDataModel.ObjectModel.Enums.CdmDataFormat.String:
                        return nameof(System.String);
                    default:
                        throw new UnknownDataTypeException();
                }
            }
        }
    }

}
