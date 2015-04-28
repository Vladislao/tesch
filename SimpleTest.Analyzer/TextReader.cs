using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTest.Analyzer
{
    public class WordDock
    {
        public List<WordBlock> Blocks { get; set; }
    }

    public class WordBlock
    {
        public string Text { get; set; }

        public List<WordLine> Lines { get; set; }
    }

    public class WordLine
    {
        public string Text { get; set; }

        public List<Word> Words { get; set; }
    }

    public class Word
    {
        public string Text { get; set; }

        public WordLine Line { get; set; }
        public WordBlock Block { get; set; }
        public WordDock Dock { get; set; }

        public Word Next { get; set; }
        public Word Previous { get; set; }

        public bool Processed { get; set; }

        /// <summary>
        /// kinda overload for get function - for lazy ctor
        /// </summary>
        /// <returns></returns>
        public string GetNew()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return Text;

            var first = Text.IndexOf('(');
            if (first == -1)
            {
                // in case it is not closed - as is
                return Text + "()";
            }

            return GetFunction();
        }

        /// <summary>
        /// returns word as function
        /// </summary>
        /// <returns></returns>
        public string GetFunction()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return Text;

            var first = Text.IndexOf('(');
            if (first == -1)
            {
                // in case it is not closed - as is
                return Text;
            }

            // retrieve params
            var prms = GetParams();

            var result = new StringBuilder();
            result.Append(Text.Substring(0, first + 1));

            foreach (var param in prms)
            {
                var txt = param;
                if (param.StartsWith("any"))
                {
                    var type = "object";
                    if (param != "any")
                        type = param.Substring(4);

                    txt = string.Format("It.IsAny<{0}>()", type);
                }
                result.Append(txt + ", ");
            }
            result.Remove(result.Length - 2, 2);
            result.Append(')');

            return result.ToString();
        }

        /// <summary>
        /// returns parameters for word
        /// </summary>
        /// <returns></returns>
        public List<string> GetParams()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return new List<string>();

            var first = Text.IndexOf('(');
            var last = Text.IndexOf(')');

            if (first < 0 || last < 0 || first >= last)
                return new List<string>();

            var inside = Text.Substring(first + 1, last - first - 1);
            return inside.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        }

        /// <summary>
        /// returns word as value (new obj or val)
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return Text;

            if (Text == "new" && Next != null)
            {
                // creating new object
                Next.Processed = true;
                if (Next.Next != null && Next.Next.Text.StartsWith("{"))
                {
                    // in case we define properties
                    Next.Next.Processed = true;
                    return string.Format("new {0} {1}", Next.Text, Next.Next.Text);
                }
                // default
                return "new " + Next.GetNew();
            }

            return Text;
        }

        /// <summary>
        /// returns word without parenthesis && lowercase
        /// </summary>
        /// <returns></returns>
        public string GetPlain()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return Text;

            var first = Text.IndexOf('(');

            return first != -1 ? Text.Substring(0, first).ToLowerInvariant() : Text.ToLowerInvariant();
        }
    }

    public class TextReader
    {
        /// <summary>
        /// make file parsable
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string FormalizeText(string text)
        {
            var whitespace = 0;
            var newline = 0;
            var brace = 0;

            bool quote = false;

            var result = new StringBuilder();
            foreach (var ch in text)
            {
                // we dont need that trash
                if (ch == '\r')
                    continue;

                // do not touch strings
                if (quote && ch != '\"')
                {
                    result.Append(ch);
                    continue;
                }

                // do not touch braces content
                if (brace > 0 && ch != '{' && ch != '}')
                {
                    // oneline it
                    if (ch != '\t' && ch != '\n')
                        result.Append(ch);
                    continue;
                }

                // we need only one whitespace between anything
                if (whitespace > 0 && char.IsWhiteSpace(ch) && ch != '\t' && ch != '\n')
                    continue;

                // remove whitespace at end
                if (whitespace > 0 && ch == '\n')
                    result.Remove(result.Length - whitespace, whitespace);

                // line never starts with whitespace
                if (newline > 0 && char.IsWhiteSpace(ch) && ch != '\t' && ch != '\n')
                    continue;

                // tabs only at beginning of string
                if (ch == '\t' && newline == 0)
                    continue;

                // if we tabbed - remove second line
                if (ch == '\t' && newline == 2)
                    result.Remove(result.Length - 1, 1);

                // no more than 2 new lines between anything
                if (newline > 1 && ch == '\n')
                    continue;

                if (brace == 0 && ch == '{')
                {
                    // if brace started - remove all newlines, tabs and whitespaces at end
                    var c = result.Length - 1;
                    while (c > 0 && char.IsWhiteSpace(result[c]))
                    {
                        result.Remove(c, 1);
                        c--;
                    }
                    result.Append(" ");
                }

                if (char.IsWhiteSpace(ch) && ch != '\t' && ch != '\n') whitespace++;
                else whitespace = 0;

                if (ch == '\n') newline++;
                else newline = 0;

                if (ch == '\"' && result[result.Length - 1] != '\\') quote = !quote;

                if (ch == '{') brace++;
                if (ch == '}') brace = brace > 1 ? brace - 1 : 0;

                result.Append(ch);
            }
            string.Join("\n", text.Split('\n').Select(s => s.Trim()));
            return result.ToString().Trim();
        }

        /// <summary>
        /// add lines and words to block
        /// </summary>
        /// <param name="dock"></param>
        /// <param name="txt"></param>
        private void CombineBlock(WordDock dock, string txt)
        {
            var lines = GetLines(txt);

            var block = new WordBlock
            {
                Text = txt,
                Lines = lines
            };

            // force deep dependency (helps a lot with inline generators)
            foreach (var line in lines)
            {
                foreach (var word in line.Words)
                {
                    word.Line = line;
                    word.Block = block;
                    word.Dock = dock;
                }
            }

            dock.Blocks.Add(block);
        }

        /// <summary>
        /// get docked blocks from text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public WordDock GetBlocks(string text)
        {
            var result = new WordDock
            {
                Blocks = new List<WordBlock>()
            };

            string txt;
            var newline = 0;
            var stringBuilder = new StringBuilder();

            foreach (var ch in text)
            {
                if (newline > 1)
                {
                    // here we go to the next block
                    txt = stringBuilder.Remove(stringBuilder.Length - 2, 2).ToString();
                    CombineBlock(result, txt);

                    stringBuilder = new StringBuilder();
                }

                // new block starts after empty string
                if (ch == '\n') newline++;
                else newline = 0;

                stringBuilder.Append(ch);
            }

            txt = stringBuilder.ToString();
            CombineBlock(result, txt);

            return result;
        }

        /// <summary>
        /// get line objects from text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<WordLine> GetLines(string text)
        {
            var lines = text.Split('\n');
            var result = new List<WordLine>();
            foreach (var line in lines)
            {
                var words = GetWords(line);
                var wordLine = new WordLine
                {
                    Text = line,
                    Words = words
                };
                result.Add(wordLine);
            }
            return result;
        }

        /// <summary>
        /// split text to words
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<string> SplitWhitespaces(string text)
        {
            var words = new List<string>();
            var wordBuilder = new StringBuilder();

            var whitespace = false;
            var quote = false;
            var parenthesis = false;
            var brace = 0;

            foreach (var ch in text)
            {
                // dont touch string
                if (quote && ch != '\"')
                {
                    wordBuilder.Append(ch);
                    continue;
                }
                // function params as one word
                if (parenthesis && ch != ')')
                {
                    wordBuilder.Append(ch);
                    continue;
                }
                // text in braces - as one word
                if (brace > 0 && ch != '}' && ch != '{')
                {
                    wordBuilder.Append(ch);
                    continue;
                }
                // found whitespace - new word
                if (whitespace)
                {
                    wordBuilder.Remove(wordBuilder.Length - 1, 1);
                    words.Add(wordBuilder.ToString());
                    wordBuilder = new StringBuilder();
                }

                if (char.IsWhiteSpace(ch)) whitespace = true;
                else whitespace = false;

                if (ch == '\"')
                    if (wordBuilder.Length > 0)
                    {
                        if (wordBuilder[wordBuilder.Length - 1] != '\\')
                        {
                            quote = !quote;
                        }
                    }
                    else
                        quote = !quote;
                
                if (ch == '(') parenthesis = true;
                if (ch == ')') parenthesis = false;

                if (ch == '{') brace++;
                if (ch == '}') brace = brace > 1 ? brace - 1 : 0;

                wordBuilder.Append(ch);
            }
            // add the last word
            words.Add(wordBuilder.ToString());
            return words;
        }

        /// <summary>
        /// get word objects from text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<Word> GetWords(string text)
        {
            Word prev = null;
            var words = SplitWhitespaces(text);
            var result = new List<Word>();

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                    continue;

                var wordObj = new Word
                {
                    Text = word,
                    Previous = prev,
                };

                if (prev != null)
                    prev.Next = wordObj;

                prev = wordObj;
                result.Add(wordObj);
            }
            return result;
        }

    }
}