using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleTest.Analyzer
{
    public class Generator
    {
        private readonly TextReader _textReader;
        public static InlineGenerators InlineGenerators = new InlineGenerators();
        public static BlockGenerators BlockGenerators = new BlockGenerators();

        public Generator()
        {
            _textReader = new TextReader();
        }

        /// <summary>
        /// generates .cs file from our .tst
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Generate(string text)
        {
            // formalize
            text = _textReader.FormalizeText(text);
            // parse
            var dock = _textReader.GetBlocks(text);
            // generate block by block
            var result = new StringBuilder();
            foreach (var block in dock.Blocks)
            {
                var firstword = block.Lines[0].Words[0];

                if (BlockGenerators.ContainsKey(firstword.Text))
                    result.Append(BlockGenerators[firstword.Text].Generate(block));
                else
                    throw new NotImplementedException();
            }
            result.Append("\t}\n}\n");

            return result.ToString();
        }
    }


}
