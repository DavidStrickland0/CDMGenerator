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
            List<MemberDeclarationSyntax> properties = new List<MemberDeclarationSyntax>();

            foreach (var attr in cdmEntity.Attributes)
            {
                if (attr is CdmTypeAttributeDefinition typeAttr)
                {
                    properties.Add(GenerateProperty(typeAttr));
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
                                properties.Add(GenerateProperty(groupTypeAttr));
                            }
                        }
                    }
                }
            }

            string comment = $"/// <summary>\n/// {cdmEntity.Description ?? "No description available."}\n/// </summary>\n";

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

            return syntaxTree.ToFullString();
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
                "operator" => true,
                "class" => true,
                "int" => true,
                "string" => true,
                "namespace" => true,
                "abstract"=>true,
                "default" => true,
                "event" => true,
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
                    case "int64":
                        return nameof(Int64);
                    //List<Object>
                    case "list":
                    case "listlookupcorrelated":
                    case "listlookupmultiple":
                    case "listlookup":
                    case "partylist":
                        return "System.Collections.Generic.List<Object>";
                    //String
                    case "addressline":
                    case "city":
                    case "colorname":
                    case "country":
                    case "county":
                    case "governmentId":
                    case "language":
                    case "languageTag":
                    case "localizedDisplayText":
                    case "localizedDisplayTextMultiple":
                    case "name":
                    case "firstname":
                    case "fullname":
                    case "gender":
                    case "ethnicity":
                    case "maritalStatus":
                    case "lastname":
                    case "middlename":
                    case "postalCode":
                    case "stateOrProvince":
                    case "timezone":
                    case "email":
                    case "phone":
                    case "phonecell":
                    case "phonefax":
                    case "colorName":
                    case "string":
                    case "tickersymbol":
                    case "url":
                        return nameof(String);
                    //Object
                    case "postalcode":
                    case "stateorprovince":
                    case "image":
                        return nameof(Object);
                    default:
                        return String.Concat(attr.DataType.NamedReference); // Fallback for unmapped types
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
