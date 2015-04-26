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
            Add(TestFunction.Name, new TestFunction());
        }
    }

    public class TearDown : IBlockGenerator
    {
        public const string Name = "teardown";

        public string Generate(WordBlock block)
        {
            var result = new StringBuilder();

            result.Append("\t\t[TearDown]\n\t\tpublic void TearDown()\n\t\t{");

            // lets make body
            foreach (var line in block.Lines.Skip(1))
            {
                result.Append("\t\t\t\t");
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

            // generate footer
            result.Append("\t\t}\n");
            return result.ToString();
        }
    }

    public class TestFunction : IBlockGenerator
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

            // lets make body
            foreach (var line in block.Lines.Skip(1))
            {
                result.Append("\t\t\t\t");
                foreach (var word in line.Words)
                {
                    if(word.Processed)
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

            result.Append("\t\t\t}\n");
            // generate footer
            result.Append("\t\t}\n");
            return result.ToString();
        }
    }
}