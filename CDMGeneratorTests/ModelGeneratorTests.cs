using CDMGenerator;

namespace CDMGeneratorTests
{
    public class ModelGeneratorTests
    {
        [Fact]
        public void Constructor_Constructs()
        {
            var ObjectUnderTest = new ModelGenerator(p => { });
            Assert.NotNull(ObjectUnderTest);
        }
        [Fact]
        public async void Generate_WithNullPath_Throws()
        {
            var ObjectUnderTest = new ModelGenerator(p => { });
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await ObjectUnderTest.Generate(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await Assert.ThrowsAsync<ArgumentException>(async () => await ObjectUnderTest.Generate(string.Empty));
        }
        [Fact]
        public async void Generate_WithInvalidPath_Throws()
        {
            var ObjectUnderTest = new ModelGenerator(p => { });
            await Assert.ThrowsAsync<ArgumentException>(async () => await ObjectUnderTest.Generate("../"));
        }
        //ModelGeneratorTests.cs
        [Fact]
        public async void Generate_WithValidPathWithInvalidContents_Throws()
        {
            var ObjectUnderTest = new ModelGenerator(p => { });
            string filePath = "..\\..\\..\\CDMGeneratorTests\\sample-data\\ModelGeneratorTests.cs";
            // Resolve the relative path to an absolute path
            string fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath)) filePath = Path.GetFullPath("..\\..\\..\\..\\CDMGeneratorTests\\sample-data\\ModelGeneratorTests.cs");
            await Assert.ThrowsAsync<ArgumentException>(async () => await ObjectUnderTest.Generate("..\\..\\..\\..\\CDMGeneratorTests\\sample-data\\ModelGeneratorTests.cs"));
        }

        [Fact]
        public async void Generate_WithValidPath()
        {
            var ObjectUnderTest = new ModelGenerator(p => {
                System.Diagnostics.Debug.WriteLine(p);
            });
            string filePath = "..\\..\\..\\CDMGeneratorTests\\sample-data\\default.manifest.cdm.json";
            // Resolve the relative path to an absolute path
            string fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath)) filePath = Path.GetFullPath("..\\..\\..\\..\\CDMGeneratorTests\\sample-data\\default.manifest.cdm.json");
            await ObjectUnderTest.Generate(filePath);

        }

    }
}