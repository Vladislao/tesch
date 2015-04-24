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

    public class TestFunction : IBlockGenerator
    {
        public const string Name = "test";

        public string Generate(WordBlock block)
        {
            var result = new StringBuilder();
            // generate header 
            result.Append("\t[Test]\n\tpublic void ");
            // get test name
            var testName = block.Lines[0].Words[1];
            result.Append(testName.Text);
            // continue header
            result.Append("()\n\t{\n\t\tusing (var mock = AutoMock.GetLoose())\n\t\t{\n");

            // lets make body
            foreach (var line in block.Lines.Skip(1))
            {
                result.Append("\t\t\t");
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

            result.Append("\t\t}\n");
            // generate footer
            result.Append("\t}\n");
            return result.ToString();
        }
    }
}