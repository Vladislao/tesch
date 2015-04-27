using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace SimpleTest.Analyzer.Tests
{
    [TestFixture]
    public class TextReaderTests
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void FormalizeText_Fixed_StillFixed()
        {
            var brokenFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "simple-fixed.txt"));
            var fixedFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "simple-fixed.txt")).Replace("\r", string.Empty);
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(brokenFile);

            Assert.IsNotNull(formalizedText);
            Assert.AreEqual(fixedFile, formalizedText);
        }

        [Test]
        public void FormalizeText_SpaceAndNewlines_Fixed()
        {
            var brokenFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "simple-broken.txt"));
            var fixedFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "simple-fixed.txt")).Replace("\r", string.Empty);
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(brokenFile);

            Assert.IsNotNull(formalizedText);
            Assert.AreEqual(fixedFile, formalizedText);
        }

        [Test]
        public void FormalizeText_TrimLines_Fixed()
        {
            var brokenFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "linestrim-broken.txt"));
            var fixedFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "simple-fixed.txt")).Replace("\r", string.Empty);
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(brokenFile);

            Assert.IsNotNull(formalizedText);
            Assert.AreEqual(fixedFile, formalizedText);
        }

        [Test]
        public void FormalizeText_TabOnlyAtStart_Fixed()
        {
            var brokenFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "tabs-broken.txt"));
            var fixedFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "simple-fixed.txt")).Replace("\r", string.Empty);
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(brokenFile);

            Assert.IsNotNull(formalizedText);
            Assert.AreEqual(fixedFile, formalizedText);
        }

        [Test]
        public void FormalizeText_NewLinesAtEnd_Fixed()
        {
            var brokenFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "newlines-atend-broken.txt"));
            var fixedFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "simple-fixed.txt")).Replace("\r", string.Empty);
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(brokenFile);

            Assert.IsNotNull(formalizedText);
            Assert.AreEqual(fixedFile, formalizedText);
        }

        [Test]
        public void FormalizeText_WhitespaceBetweenCommas_Fixed()
        {
            var brokenFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "whitespace-broken.txt"));
            var fixedFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "whitespace-fixed.txt")).Replace("\r", string.Empty);
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(brokenFile);

            Assert.IsNotNull(formalizedText);
            Assert.AreEqual(fixedFile, formalizedText);
        }

        [Test]
        public void GetBlocks_BlockStartsAfterEmptyString_Correct()
        {
            var brokenFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "formalize", "simple-fixed.txt"));
            var textReader = new TextReader();

            var formalizedText = textReader.FormalizeText(brokenFile);
            var dock = textReader.GetBlocks(formalizedText);

            Assert.IsNotNull(dock);
            Assert.AreEqual(5, dock.Blocks.Count);

            Assert.AreEqual("use PATH_TO_CSFILE", dock.Blocks[0].Text);
            Assert.AreEqual("setup-fixture", dock.Blocks[1].Text);
            Assert.AreEqual("setup", dock.Blocks[2].Text);
            Assert.AreEqual("teardown", dock.Blocks[3].Text);
            Assert.AreEqual("test SendContractToApi_ParsableXml_Sent\n\tmock IParserFactory setup ParseContract(any) returns new ContractModel\n\tact(\"string\")\n\tmock IMongoRepository verify Publish(any) called once", dock.Blocks[4].Text);
        }

        [Test]
        public void Word_GetValue_NewObject_Correct()
        {
            var word = new Word
            {
                Text = "new",
                Next = new Word
                {
                    Text = "ContractModel"
                }
            };
            var value = word.GetValue();

            Assert.AreEqual("new ContractModel()", value);
        }

        [Test]
        public void Word_GetValue_NewObjectWithParams_Correct()
        {
            var word = new Word
            {
                Text = "new",
                Next = new Word
                {
                    Text = "ContractModel(522,\"test\")"
                }
            };
            var value = word.GetValue();

            Assert.AreEqual("new ContractModel(522,\"test\")", value);
        }

        [Test]
        public void Word_GetValue_Number_Correct()
        {
            var word = new Word
            {
                Text = "24"
            };
            var value = word.GetValue();

            Assert.AreEqual("24", value);
        }

        [Test]
        public void Word_GetValue_Str_Correct()
        {
            var word = new Word
            {
                Text = "\"24\""
            };
            var value = word.GetValue();

            Assert.AreEqual("\"24\"", value);
        }

        [Test]
        public void SplitWhitespaces_WhitespaceInString_Correct()
        {
            var textReader = new TextReader();

            var words = textReader.SplitWhitespaces("act(\"string \\\" quote \\\"           test\")");

            Assert.AreEqual(1, words.Count);
            Assert.AreEqual("act(\"string \\\" quote \\\"           test\")", words[0]);
        }

        [Test]
        public void SplitWhitespaces_TwoWords_Correct()
        {
            var textReader = new TextReader();

            var words = textReader.SplitWhitespaces("act(\"string \\\" quote \\\"           test\") test");

            Assert.AreEqual(2, words.Count);
            Assert.AreEqual("act(\"string \\\" quote \\\"           test\")", words[0]);
            Assert.AreEqual("test", words[1]);
        }
    }
}
