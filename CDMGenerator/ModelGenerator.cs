using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CommonDataModel.ObjectModel.Cdm;
using Microsoft.CommonDataModel.ObjectModel.Storage;
using Microsoft.CommonDataModel.ObjectModel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDMGenerator
{
    public class ModelGenerator
    {
        private Action<CompilationUnitSyntax> pocoHandler;

        public ModelGenerator(Action<CompilationUnitSyntax> pocoHandler)
        {
            this.pocoHandler = pocoHandler;
        }

        public async Task Generate(string schemaRoot, string ManifestFile)
        {
            ArgumentNullException.ThrowIfNull(ManifestFile);
            var ManifestFilePath = Path.GetFullPath(Path.Combine(schemaRoot, ManifestFile));
            ArgumentNullException.ThrowIfNullOrEmpty(Path.Combine(schemaRoot, ManifestFilePath));
            if (!File.Exists(ManifestFilePath)) throw new ArgumentException($"{ManifestFilePath} does not exist");
            string manifestPath = string.Empty;

            var cdmCorpus = new CdmCorpusDefinition();
            // set callback to receive error and warning logs.
            cdmCorpus.SetEventCallback(new EventCallback
            {
                Invoke = (level, message) =>
                {
                    Console.WriteLine(message);
                }
            }, CdmStatusLevel.Warning);

            // Storage adapter pointing to the target local manifest location. 
            cdmCorpus.Storage.Mount("local", new LocalAdapter(schemaRoot));
            cdmCorpus.Storage.DefaultNamespace = "local";
            Console.WriteLine($"\nLoading manifest {manifestPath} ...");
            await processManifest(cdmCorpus, ManifestFile);

        }

        private  List<string> manifestsProcessed = new List<string>();
        private async Task processManifest(CdmCorpusDefinition cdmCorpus, string manifestPath)
        {
            if(manifestsProcessed.Contains(manifestPath)) return;
            manifestsProcessed.Append(manifestPath);
            CdmManifestDefinition manifest = await cdmCorpus.FetchObjectAsync<CdmManifestDefinition>(manifestPath);

            if (manifest == null)
            {
                manifest = await cdmCorpus.FetchObjectAsync<CdmManifestDefinition>(Path.Combine("core/applicationCommon",manifestPath));
            }

            if (manifest.Entities.Count == 0 && manifest.SubManifests.Count == 0) throw new ArgumentException($"Manifest {manifestPath} does not contain Entities or SubManifests.");
            foreach (var subManifest in manifest.SubManifests)
            {
                await processManifest(cdmCorpus, cdmCorpus.Storage.CreateAbsoluteCorpusPath(subManifest.Definition, manifest));
            }
            foreach (var entity in manifest.Entities)
            {
                await processEntity(cdmCorpus, manifest, entity.EntityPath);
            }
        }
        List<string> entitiesProcessed = new List<string>();

        private async Task processEntity(CdmCorpusDefinition cdmCorpus, CdmManifestDefinition manifest, string entityPath)
        {
            var entitySelected = await cdmCorpus.FetchObjectAsync<CdmEntityDefinition>(entityPath,manifest);
            if (entitySelected == null)
            {
                return;
            }
            string fullyQualifiedName = entitySelected.EntityName;
            if (entitiesProcessed.Contains(entitySelected.AtCorpusPath)) return;
            entitiesProcessed.Add(entitySelected.AtCorpusPath);
            var poco = await CdmToPocoGenerator.GeneratePocoClass(entitySelected, manifest, async (entityPath) => await processDocument(cdmCorpus, manifest, entityPath));
            pocoHandler(poco);
        }
        private async Task<string> processDocument(CdmCorpusDefinition cdmCorpus, CdmManifestDefinition manifest, string entityPath)
        {
            List<string> searchPaths = new List<string> 
            {
            "core/applicationCommon/",
            "core/applicationCommon/foundationCommon/crmCommon/"
            };
            foreach(var path in searchPaths) 
            {
                var fullEntityPath = String.Concat(path, entityPath);
                var cdmDocumentDefinition = await cdmCorpus.FetchObjectAsync<CdmDocumentDefinition>(fullEntityPath);
                if (cdmDocumentDefinition != null)
                {
                    var entitySelected = cdmDocumentDefinition.Definitions.FirstOrDefault() as CdmEntityDefinition;
                    if (entitySelected == null) return nameof(Object);
                    string fullyQualifiedName = entitySelected.EntityName;
                    var poco = await CdmToPocoGenerator.GeneratePocoClass(entitySelected, manifest, async entityPath => await processDocument(cdmCorpus, manifest, entityPath));
                    if (!entitiesProcessed.Contains(entitySelected.AtCorpusPath))
                    {
                        entitiesProcessed.Add(entitySelected.AtCorpusPath);
                        pocoHandler(poco);
                    }
                    // Extract namespace and class name
                    var namespaceDeclaration = poco.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                    var classDeclaration = poco.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

                    return $"{namespaceDeclaration.Name}.{classDeclaration.Identifier.ValueText}" ;                    return fullEntityPath;
                }
            }
            return nameof(Object);
        }
    }
}