using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SimpleTest.Analyzer.Tests
{
    [TestFixture]
    public class GeneratorsTests
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void Generator_Simple_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-cs", "simple.txt")).Replace("\r", string.Empty);

            var testFunction = new Generator();
            var generated = testFunction.Generate(file);

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void Generator_Simple2_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple-2.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-cs", "simple-2.txt")).Replace("\r", string.Empty);

            var testFunction = new Generator();
            var generated = testFunction.Generate(file);

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void DefBlock_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple-2.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "def-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new DefBlock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[5]);

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void VarBlock_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple-2.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "varblock-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new VarBlock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[4]);

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void TestBlock_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "testfunction-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new TestBlock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks.Last());

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void SetUp_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "setup-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new SetUpBlock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[2]);

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void SetUpFixture_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "setup-fixture-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new SetUpFixtureBlock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[1]);

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void Use_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "use-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new UseBlock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[0]);

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void TearDown_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "teardown-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new TearDownBlock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[3]);

            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void Mock_SimpleInterface_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Mock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks.Last().Lines[1].Words.First());

            Assert.AreEqual("mock.Mock<IParserFactory>()", generated);
        }

        [Test]
        public void Setup_Empty_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Setup();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks.Last().Lines[1].Words[2]);

            Assert.AreEqual(".Setup(f => f.ParseContract(It.IsAny<object>()))", generated);
        }

        [Test]
        public void Returns_NewModel_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Returns();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks.Last().Lines[1].Words[4]);

            Assert.AreEqual(".Returns(() => new ContractModel())", generated);
        }

        [Test]
        public void Act_WithParameter_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Act();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks.Last().Lines[2].Words[0]);

            Assert.AreEqual("var actor = mock.Create<Sender>();\n\t\t\t\tactor.SendContractToApi(\"string\")", generated);
        }

        [Test]
        public void New_Class_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple-2.txt"));
            var testFunction = new New();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[5].Lines[1].Words[2]);

            Assert.AreEqual("new Mock<IRabbitMqProvider>()", generated);
        }

        [Test]
        public void Provide_Object_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple-2.txt"));
            var testFunction = new Provide();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[5].Lines[2].Words[0]);

            Assert.AreEqual("mock.Provide<IFileSystem>(new MockFileSystem())", generated);
        }
    }
}
