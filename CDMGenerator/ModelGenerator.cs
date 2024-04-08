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
        private Action<string> pocoHandler;

        public ModelGenerator(Action<string> pocoHandler) { 
            this.pocoHandler = pocoHandler; 
        }

        public async Task Generate(string schemaRoot, string ManifestFile)
        {
            ArgumentNullException.ThrowIfNull(ManifestFile);
            var ManifestFilePath = Path.GetFullPath(Path.Combine(schemaRoot, ManifestFile));
            ArgumentNullException.ThrowIfNullOrEmpty(Path.Combine(schemaRoot,ManifestFilePath));
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
            cdmCorpus.Storage.Mount("local", new LocalAdapter(Path.GetDirectoryName(schemaRoot)));
            cdmCorpus.Storage.DefaultNamespace = "local";
            await processManifest(cdmCorpus, ManifestFile);

        }

        private async Task processManifest(CdmCorpusDefinition cdmCorpus, string manifestPath)
        {
            Console.WriteLine($"\nLoading manifest {manifestPath} ...");

            CdmManifestDefinition manifest = await cdmCorpus.FetchObjectAsync<CdmManifestDefinition>(manifestPath);

            if (manifest == null) throw new ArgumentException($"Unable to load manifest {manifestPath}.");

            if (manifest.Entities.Count == 0 && manifest.SubManifests.Count == 0) throw new ArgumentException($"Manifest {manifestPath} does not contain Entities or SubManifests.");
            foreach (var subManifest in manifest.SubManifests)
            {
                await processManifest(cdmCorpus, cdmCorpus.Storage.CreateAbsoluteCorpusPath(subManifest.Definition, manifest));
            }
            foreach (var entity in manifest.Entities)
            {
                var entSelected = await cdmCorpus.FetchObjectAsync<CdmEntityDefinition>(entity.EntityPath, manifest); // gets the entity object from the doc
                var poco = CdmToPocoGenerator.GeneratePocoClass(entSelected,manifest);
                pocoHandler(poco);
            }
        }
    }
}
