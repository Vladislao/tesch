using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleTest.Analyzer.Tests
{
    [TestClass]
    public class GeneratorsTests
    {
        [TestInitialize]
        public void SetUp()
        {
            
        }
        
        [TestMethod]
        public void TestFunction_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "testfunction-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new TestFunction();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks.Last());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void TearDown_Body_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var expected = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "generators", "teardown-generated.txt")).Replace("\r", string.Empty);
            var testFunction = new TearDown();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks[3]);

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
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

        [TestMethod]
        public void Setup_WithAnyParam_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Setup();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var dock = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(dock.Blocks.Last().Lines[1].Words[2]);

            Assert.AreEqual(".Setup(f => f.ParseContract(It.IsAny<object>()))", generated);
        }

        [TestMethod]
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

        [TestMethod]
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

    }
}
