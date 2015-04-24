using System;
using System.Collections.Generic;
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
            var blocks = _textReader.GetBlocks(text);
            // generate block by block
            var result = new StringBuilder();
            foreach (var block in blocks)
            {
                var firstword = block.Lines[0].Words[0];

                if (BlockGenerators.ContainsKey(firstword.Text))
                    result.Append(BlockGenerators[firstword.Text].Generate(block));
                else
                    throw new NotImplementedException();
            }

            return result.ToString();
        }
    }

    public class BlockGenerators : Dictionary<string, IBlockGenerator>
    {
        public BlockGenerators()
        {
            Add(TestFunction.Name, new TestFunction());
        }
    }

    public class InlineGenerators : Dictionary<string, IInlineGenerator>
    {
        public InlineGenerators()
        {
            Add(Mock.Name, new Mock());
            Add(Setup.Name, new Setup());
            Add(Returns.Name, new Returns());
        }

    }

    public interface IInlineGenerator
    {
        string Generate(WordLine line, Word word);
    }

    public interface IBlockGenerator
    {
        string Generate(WordBlock block);
    }

    public class Mock : IInlineGenerator
    {
        public const string Name = "mock";

        public string Generate(WordLine line, Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;
            return string.Format("mock.Mock<{0}>()", word.Next.Text);
        }
    }

    public class Setup : IInlineGenerator
    {
        public const string Name = "setup";
        //.Setup(f => f.Something(It.IsAny<object>()))
        public string Generate(WordLine line, Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;

            return string.Format(".Setup(f => f.{0})", word.Next.GetFunction());
        }
    }

    public class Returns : IInlineGenerator
    {
        public const string Name = "returns";
        //.Returns(() => new ContractModel())
        public string Generate(WordLine line, Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;

            return string.Format(".Returns(() => {0})", word.Next.GetValue());
        }
    }

//    public class New : IInlineGenerator
//    {
//        public const string Name = "new";
//        // new ContractModel()
//        public string Generate(WordLine line, Word word)
//        {
//            // not valid actually - error?
//            if (word.Next == null)
//                return null;
//
//            return string.Format("new {0}", word.Next.GetFunction());
//        }
//    }

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
                    var txt = word.Text.ToLowerInvariant();
                    if (Generator.InlineGenerators.ContainsKey(txt))
                        result.Append(Generator.InlineGenerators[txt].Generate(line, word));
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
