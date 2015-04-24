using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTest.Analyzer
{
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

        public Word Next { get; set; }
        public Word Previous { get; set; }

        public string GetFunction()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return Text;

            var first = Text.IndexOf('(');
            if (first == -1)
            {
                // in case it is not closed - just empty call
                return Text + "()";
            }
            
            // retrieve params
            var prms = GetParams();

            var result = new StringBuilder();
            result.Append(Text.Substring(0, first + 1));

            foreach (var param in prms)
            {
                var txt = param;
                if (param.Contains("any"))
                {
                    switch (param)
                    {
                        case "any-string":
                            txt = "It.IsAny<string>()";
                            break;
                        default:
                            txt = "It.IsAny<object>()";
                            break;
                    }
                }
                result.Append(txt + ",");
            }
            result.Remove(result.Length - 1, 1);
            result.Append(')');

            return result.ToString();
        }

        public List<string> GetParams()
        {
            if(string.IsNullOrWhiteSpace(Text))
                return new List<string>();

            var first = Text.IndexOf('(');
            var last = Text.IndexOf(')');

            if(first < 0 || last < 0 || first >= last)
                return new List<string>();
            
            var inside = Text.Substring(first + 1, last - first - 1);
            return inside.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public string GetValue()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return Text;

            switch (Text)
            {
                case "new":
                    return Next != null ? "new " + Next.GetFunction() : string.Empty;
                default:
                    return Text;
            }
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

            bool parenthesis = false;
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

                if (parenthesis && char.IsWhiteSpace(ch))
                    continue;

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

                if (char.IsWhiteSpace(ch) && ch != '\t' && ch != '\n') whitespace++;
                else whitespace = 0;

                if (ch == '\n') newline++;
                else newline = 0;

                if (ch == '(') parenthesis = true;
                if (ch == ')') parenthesis = false;
                
                if (ch == '\"' && result[result.Length - 1] != '\\') quote = !quote;

                result.Append(ch);
            }
            string.Join("\n", text.Split('\n').Select(s => s.Trim()));
            return result.ToString().Trim();
        }

        public List<WordBlock> GetBlocks(string text)
        {
            var result = new List<WordBlock>();
            string txt;
            List<WordLine> lines;

            var newline = 0;
            var stringBuilder = new StringBuilder();

            foreach (var ch in text)
            {
                if (newline > 1)
                {
                    // here we go to the next block
                    txt = stringBuilder.Remove(stringBuilder.Length - 2, 2).ToString();
                    lines = GetLines(txt);

                    var block = new WordBlock
                    {
                        Text = txt,
                        Lines = lines
                    };

                    result.Add(block);

                    stringBuilder = new StringBuilder();
                }

                // new block starts after empty string
                if (ch == '\n') newline++;
                else newline = 0;

                stringBuilder.Append(ch);
            }

            txt = stringBuilder.ToString();
            lines = GetLines(txt);
            // here we go to the last block
            result.Add(new WordBlock
            {
                Text = txt,
                Lines = lines
            });

            return result;
        }

        public List<WordLine> GetLines(string text)
        {
            var lines = text.Split('\n');
            var result = new List<WordLine>();
            foreach (var line in lines)
            {
                var wordLine = new WordLine
                {
                    Text = line,
                    Words = GetWords(line)
                };
                result.Add(wordLine);
            }
            return result;
        }

        public List<string> SplitWhitespaces(string text)
        {
            var words = new List<string>();
            var whitespace = false;
            var quote = false;
            var wordBuilder = new StringBuilder();
            foreach (var ch in text)
            {
                // dont touch string
                if (quote && ch != '\"')
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

                if (ch == '\"' && wordBuilder[wordBuilder.Length - 1] != '\\') quote = !quote;

                wordBuilder.Append(ch);
            }
            // add the last word
            words.Add(wordBuilder.ToString());
            return words;
        } 

        public List<Word> GetWords(string text)
        {
            Word prev = null;
            var words = SplitWhitespaces(text);
            var result = new List<Word>();

            foreach (var word in words)
            {
                if(string.IsNullOrWhiteSpace(word))
                    continue;

                var wordObj = new Word
                {
                    Text = word,
                    Previous = prev
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