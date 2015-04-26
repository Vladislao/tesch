using System.Collections.Generic;
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
            Add(SetUpBlock.Name, new SetUpBlock());
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

    public class SetUpFixtureBlock : IBlockGenerator
    {
        public const string Name = "setup-fixture";

        public string Generate(WordBlock block)
        {
            var result = new StringBuilder();
            result.Append("\t\t[TestFixtureSetUp]\n\t\tpublic void SetUpFixture()\n\t\t{");

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
            result.Append("\t\t[SetUp]\n\t\tpublic void SetUp()\n\t\t{");

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

            result.Append("\t\t[TearDown]\n\t\tpublic void TearDown()\n\t\t{");

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
            result.Append("\t\t[Test]\n\t\tpublic void ");
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
}