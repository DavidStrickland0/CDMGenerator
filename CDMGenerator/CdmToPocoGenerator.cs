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
        public static string GeneratePocoClass(CdmEntityDefinition cdmEntity)
        {
            // Only include CdmTypeAttributeDefinition for this example
            var properties = cdmEntity.Attributes
                .Where(attr => attr is CdmTypeAttributeDefinition)
                .Cast<CdmTypeAttributeDefinition>()
                .Select(attr => GenerateProperty(attr))
                .ToArray();

            var classDeclaration = SyntaxFactory.ClassDeclaration(cdmEntity.EntityName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(properties);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("YourNamespace"))
                .AddMembers(classDeclaration);

            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();

            return syntaxTree.ToFullString();
        }

        private static MemberDeclarationSyntax GenerateProperty(CdmTypeAttributeDefinition attr)
        {
            // Simplified mapping, you'll need to tailor this to your data types
            string cSharpType = MapCdmTypeToCSharpType(attr);

            return SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(cSharpType), attr.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }

        private static string MapCdmTypeToCSharpType(CdmTypeAttributeDefinition attr)
        {
            // This function should map the CDM data types to the corresponding C# data types
            // This example only handles a few cases for demonstration purposes
            switch (attr.DataType?.NamedReference?.ToLower())
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
    }

}
