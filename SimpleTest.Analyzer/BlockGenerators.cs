using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleTest.Analyzer
{
    public interface IBlockGenerator
    {
        string Generate(WordBlock block);
    }

    public class BlockGenerators : Dictionary<string, IBlockGenerator>
    {
        public BlockGenerators()
        {
            Add(TestBlock.Name, new TestBlock());
            Add(TearDownBlock.Name, new TearDownBlock());
            Add(SetUpFixtureBlock.Name, new SetUpFixtureBlock());
            Add(SetUpBlock.Name, new SetUpBlock());
            Add(UseBlock.Name, new UseBlock());
            Add(VarBlock.Name, new VarBlock());
        }
    }

    public class VarBlock : IBlockGenerator
    {
        public const string Name = "var";

        //private Mock<IRabbitMqProvider> _queue;

        public string Generate(WordBlock block)
        {
            return string.Format("private {0} _{1};", block.Lines[0].Words[1].Text, block.Lines[0].Words[2].Text);
        }
    }

    public class UseBlock : IBlockGenerator
    {
        public const string Name = "use";

        //usings
        //
        //namespace Collector223.ApiConnector.Tests
        //{
        //    [TestFixture]
        //    public class SenderTests
        //    {

        public string Generate(WordBlock block)
        {
            var result = new StringBuilder();

            var classpath = block.Lines[0].Words[2].Text;
            var lines = File.ReadAllLines(classpath);

            var classspace = string.Empty;
            foreach (var line in lines)
            {
                var formed = line.Trim().ToLowerInvariant();
                if (formed.StartsWith("using"))
                    result.Append(line).Append("\n");
                if (formed.StartsWith("namespace"))
                    classspace = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            var classname = block.Lines[0].Words[1].Text;

            result.Append(string.Format("\nnamespace {0}.Tests\n{{\n\t[TestFixture]\n\tpublic class {1}Tests\n\t{{", classspace, classname));

            return result.ToString();
        }
    }

    public class SetUpFixtureBlock : IBlockGenerator
    {
        public const string Name = "setup-fixture";

        public string Generate(WordBlock block)
        {
            var result = new StringBuilder();
            result.Append("\n\t\t[TestFixtureSetUp]\n\t\tpublic void SetUpFixture()\n\t\t{");

            result.Append(block.ProcessLines());
            // generate footer
            result.Append("\t\t}\n");
            return result.ToString();
        }
    }

    public class SetUpBlock : IBlockGenerator
    {
        public const string Name = "setup";

        public string Generate(WordBlock block)
        {
            var result = new StringBuilder();
            result.Append("\n\t\t[SetUp]\n\t\tpublic void SetUp()\n\t\t{");

            result.Append(block.ProcessLines());
            // generate footer
            result.Append("\t\t}\n");
            return result.ToString();
        }
    }

    public class TearDownBlock : IBlockGenerator
    {
        public const string Name = "teardown";

        public string Generate(WordBlock block)
        {
            var result = new StringBuilder();

            result.Append("\n\t\t[TearDown]\n\t\tpublic void TearDown()\n\t\t{");

            result.Append(block.ProcessLines());

            // generate footer
            result.Append("\t\t}\n");
            return result.ToString();
        }
    }

    public class TestBlock : IBlockGenerator
    {
        public const string Name = "test";

        public string Generate(WordBlock block)
        {
            var result = new StringBuilder();
            // generate header 
            result.Append("\n\t\t[Test]\n\t\tpublic void ");
            // get test name
            var testName = block.Lines[0].Words[1];
            result.Append(testName.Text);
            // continue header
            result.Append("()\n\t\t{\n\t\t\tusing (var mock = AutoMock.GetLoose())\n\t\t\t{\n");

            result.Append(block.ProcessLines("\t\t\t\t"));

            result.Append("\t\t\t}\n");
            // generate footer
            result.Append("\t\t}\n");
            return result.ToString();
        }
    }

    public static class BlockExtensions
    {
        public static string ProcessLines(this WordBlock block, string tab = "\t\t\t")
        {
            var result = new StringBuilder();
            // lets make body
            foreach (var line in block.Lines.Skip(1))
            {
                result.Append(tab);
                foreach (var word in line.Words)
                {
                    if (word.Processed)
                        continue;

                    var txt = word.GetPlain();
                    if (Generator.InlineGenerators.ContainsKey(txt))
                        result.Append(Generator.InlineGenerators[txt].Generate(word));
                }
                result.Append(";\n");
            }
            // in case nothing - newline
            if (block.Lines.Count < 2)
                result.Append("\n");

            return result.ToString();
        }
    }
}