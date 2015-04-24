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
            var wordBlocks = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(wordBlocks.Last());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void Mock_TwoWords_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Mock();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var wordBlocks = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(wordBlocks.Last().Lines[1], wordBlocks.Last().Lines[1].Words.First());

            Assert.AreEqual("mock.Mock<IParserFactory>()", generated);
        }

        [TestMethod]
        public void Setup_TwoWords_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Setup();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var wordBlocks = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(wordBlocks.Last().Lines[1], wordBlocks.Last().Lines[1].Words[2]);

            Assert.AreEqual(".Setup(f => f.ParseContract(It.IsAny<object>()))", generated);
        }

        [TestMethod]
        public void Returns_NewModel_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Returns();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var wordBlocks = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(wordBlocks.Last().Lines[1], wordBlocks.Last().Lines[1].Words[4]);

            Assert.AreEqual(".Returns(() => new ContractModel())", generated);
        }

        [TestMethod]
        public void Act_TwoWords_Generated()
        {
            var file = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "files-tst", "simple.txt"));
            var testFunction = new Returns();
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(file);
            var wordBlocks = textReader.GetBlocks(formalizedText);
            var generated = testFunction.Generate(wordBlocks.Last().Lines[1], wordBlocks.Last().Lines[1].Words[4]);

            Assert.AreEqual(".Returns(() => new ContractModel())", generated);
        }
    }
}
