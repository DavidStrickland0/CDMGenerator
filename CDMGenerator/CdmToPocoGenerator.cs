namespace CDMGenerator
{
    using Microsoft.CommonDataModel.ObjectModel.Cdm;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;
    using System.Linq;
    using System.Globalization;

    public class CdmToPocoGenerator
    {
        /// <summary>
        /// Should return the Fully Qualified name of the type created
        /// </summary>
        static Func<string,Task<string>> processDocument;

        /// <summary>
        /// Returns the Compilation unit that will be written out as a file for the specified entity. When a subdocument needs to be used to create a new type
        /// the path to the subdocument is passed to the ProcessDocument Method. The Return from ProcessDocument is the data type used for compilation.
        /// </summary>
        /// <param name="cdmEntity"></param>
        /// <param name="manifest"></param>
        /// <param name="processDocument"></param>
        /// <returns></returns>
        public async static Task<CompilationUnitSyntax> GeneratePocoClass(CdmEntityDefinition cdmEntity, CdmManifestDefinition manifest, Func<string,Task<string>> processDocument)
        {
            CdmToPocoGenerator.processDocument = processDocument;
            List<MemberDeclarationSyntax> properties = new List<MemberDeclarationSyntax>();

            foreach (var attr in cdmEntity.Attributes)
            {
                if (attr is CdmTypeAttributeDefinition typeAttr)
                {
                    properties.Add(await GenerateProperty(typeAttr));
                }
                else if (attr is CdmAttributeGroupReference groupRef)
                {
                    var attributeGroup = groupRef.ExplicitReference as CdmAttributeGroupDefinition;
                    if (attributeGroup != null && attributeGroup.Members != null)
                    {
                        foreach (var groupAttr in attributeGroup.Members)
                        {
                            if (groupAttr is CdmTypeAttributeDefinition groupTypeAttr)
                            {
                                properties.Add(await GenerateProperty(groupTypeAttr));
                            }
                        }
                    }
                }
            }

            string comment = $"/// <summary>\n/// {cdmEntity.Description ?? cdmEntity.DisplayName??"No description available."}\n/// </summary>\n";

            var classDeclaration = SyntaxFactory.ClassDeclaration(cdmEntity.EntityName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(properties.ToArray())
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(comment));

            // Assuming GetManifestFullPath is a method to get the full path of the manifest
            string manifestFullPath = ExtractFullPathFromEntity(cdmEntity);
            string sanitizedNamespace = SanitizeFullPathAsNamespace(manifestFullPath);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(sanitizedNamespace))
                .AddMembers(classDeclaration);

            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();

            return syntaxTree;
        }

        private static string ExtractFullPathFromEntity(CdmEntityDefinition cdmEntity)
        {
            var document = cdmEntity.InDocument;
            var folder = document.Owner as CdmFolderDefinition;
            var folders = new List<string>();
            while (folder != null)
            {
                if (!string.IsNullOrEmpty(folder.Name))
                {
                    folders.Add(folder.Name);
                }
                folder = folder.Owner as CdmFolderDefinition;
            }
            folders.Reverse();
            return string.Join("/", folders);
        }

        private static string SanitizeFullPathAsNamespace(string fullPath)
        {
            // Replace directory separators and invalid characters with valid namespace parts
            return fullPath
                .Replace("\\", ".") // For Windows paths
                .Replace("/", ".")  // For UNIX/Linux paths
                .Replace(" ", "_")  // Replace spaces with underscores
                                    // Add more replacements as necessary
                .Trim('.');
        }


        private async static Task<MemberDeclarationSyntax> GenerateProperty(CdmTypeAttributeDefinition attr)
        {
            // Determine the C# type for the CDM attribute
            string cSharpType = await MapCdmTypeToCSharpType(attr); // Assuming a method that maps CDM data formats to C# types

            // Check if attribute name is a C# reserved keyword and prepend with '@' if necessary
            string propertyName = IsCSharpKeyword(attr.Name) ? "@" + attr.Name : attr.Name;
            var className = attr.Owner.Owner.Owner.FetchObjectDefinitionName();
            propertyName = propertyName == className ? string.Concat("_",propertyName) : propertyName;

            // Prepare the XML comment based on the attribute's description
            string comment = $"/// <summary>\n/// {attr.Description ?? attr.DisplayName ?? "No description available."}\n/// </summary>\n";

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
                "operator" => true,
                "class" => true,
                "int" => true,
                "string" => true,
                "namespace" => true,
                "abstract" => true,
                "default" => true,
                "event" => true,
                // Add other keywords as necessary
                _ => false,
            };
        }
        // The MapCdm Method is used to determine the type of a known entity or initiate the creation of an unknown entity. 
        // MapCdm should return the fully qualified type name.
        private async static Task<string> MapCdmTypeToCSharpType(CdmTypeAttributeDefinition attr)
        {
            // Check for the "is.linkedEntity.identifier" trait and process accordingly
            var linkedEntityTrait = attr.AppliedTraits.FirstOrDefault(t => t.NamedReference == "is.linkedEntity.identifier");
            if (linkedEntityTrait is CdmTraitReference traitRef && traitRef.Arguments.Count > 0)
            {
                return await GetLinkedEntityType(traitRef);
            }

            // This function should map the CDM data types to the corresponding C# data types
            // This example only handles a few cases for demonstration purposes
            if (attr.DataType != null)
            {
                //based on meanings.cdm.json
                switch (attr.DataType.NamedReference?.ToLower())
                {
                    //BigInteger
                    case "biginteger":
                        return "System.Numerics.BigInteger";
                    //Boolean
                    case "boolean":
                        return nameof(Boolean);
                    //DateTime
                    case "date":
                    case "time":
                    case "datetime":
                        return nameof(DateTime);
                    //DateTimeOffset
                    case "datetimeoffset":
                        return nameof(DateTimeOffset);
                    //Decimal
                    case "basecurrency":
                    case "currency":
                    case "decimal":
                        return nameof(Decimal);
                    //Double
                    case "latitude":
                    case "longitude":
                    case "double":
                        return nameof(Double);
                    //EntityId
                    case "entityid":
                        return nameof(Guid);
                    //Guid
                    case "guid":
                        return nameof(Guid);
                    //Int16
                    case "minutes":
                    case "smallinteger":
                    case "int16":
                        return nameof(Int16);
                    //Int32
                    case "age":
                    case "day":
                    case "week":
                    case "tenday":
                    case "month":
                    case "quarter":
                    case "trimester":
                    case "year":
                    case "int32":
                    case "integer":
                    case "displayOrder":
                        return nameof(Int32);
                    //Int64
                    case "positivenumber":
                    case "int64":
                        return nameof(Int64);
                    //List<Object>
                    case "list":
                    case "listlookupcorrelated":
                    case "listlookupmultiple":
                    case "listlookup":
                    case "partylist":
                        return "global::System.Collections.Generic.List<Object>";
                    //Object
                    case "image":
                        return nameof(Object);
                    default:
                        return nameof(String);
                }
            }
            else
            {
                throw new UnknownDataTypeException();
            }
        }
        //Get LinkedEntityType Should Return the Fully Qualified name of the type
        private async static Task<string> GetLinkedEntityType(CdmTraitReference trait)
        {
            var argument = trait.Arguments.FirstOrDefault();
            if (argument?.Value is CdmEntityReference entityRef)
            {
                if (entityRef.ExplicitReference is CdmConstantEntityDefinition constantEntityDefinition)
                {
                    var firstConstantValueList = constantEntityDefinition.ConstantValues.FirstOrDefault();
                    if (firstConstantValueList != null && firstConstantValueList.Any())
                    {
                        var firstValue = firstConstantValueList.FirstOrDefault();
                        if (!string.IsNullOrEmpty(firstValue))
                        {
                            // Split the path on '/'
                            var pathParts = firstValue.Split('/');

                            if (pathParts.Length > 1)
                            {
                                // Take all but the last two parts for the namespace, ignoring the next-to-last part
                                var manifestFileParts = pathParts.Take(pathParts.Length - 1)
                                                             .Select(part => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(part.ToLower()))
                                                             .ToList();

                                return await processDocument(string.Join("/", manifestFileParts));
                            }
                            else
                            {
                                // If there is only one part, return it as it is without altering the case
                                return pathParts[0];
                            }
                        }
                    }
                }
            }
            return "UnknownEntity"; // Return a placeholder if unable to resolve the entity name
        }
    }

}
