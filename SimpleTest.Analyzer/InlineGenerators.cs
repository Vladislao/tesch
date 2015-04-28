using System.Collections.Generic;

namespace SimpleTest.Analyzer
{
    public interface IInlineGenerator
    {
        string Generate(Word word);
    }

    public class InlineGenerators : Dictionary<string, IInlineGenerator>
    {
        public InlineGenerators()
        {
            Add(Mock.Name, new Mock());
            Add(Setup.Name, new Setup());
            Add(Returns.Name, new Returns());
            Add(Act.Name, new Act());
            Add(Called.Name, new Called());
            Add(Verify.Name, new Verify());
            Add(New.Name, new New());
            Add(Provide.Name, new Provide());
            Add(SetTo.Name, new SetTo());
            Add(Throws.Name, new Throws());
        }

    }

    public class Throws : IInlineGenerator
    {
        public const string Name = "throws";

        public string Generate(Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;

            word.Next.Processed = true;

            return string.Format(".Throws<{0}>()", word.Next.Text);
        }
    }


    public class SetTo : IInlineGenerator
    {
        public const string Name = "=";

        public string Generate(Word word)
        {
            return " = ";
        }
    }

    public class Provide : IInlineGenerator
    {
        public const string Name = "provide";

        public string Generate(Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;
            if (word.Next.Next == null)
                return null;

            var what = word.Next;
            var with = what.Next;

            word.Processed = true;
            what.Processed = true;
            with.Processed = true;

            return string.Format("mock.Provide<{0}>({1})", what.Text, with.GetValue());
        }
    }

    public class New : IInlineGenerator
    {
        public const string Name = "new";

        public string Generate(Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;

            var next = word.Next;
            word.Processed = true;
            next.Processed = true;

            return string.Format("new {0}()", next.Text);
        }
    }

    public class Mock : IInlineGenerator
    {
        public const string Name = "mock";

        public string Generate(Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;

            var next = word.Next;
            word.Processed = true;
            next.Processed = true;

            return string.Format("mock.Mock<{0}>()", next.Text);
        }
    }

    public class Setup : IInlineGenerator
    {
        public const string Name = "setup";
        //.Setup(f => f.Something(It.IsAny<object>()))
        public string Generate(Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;

            var next = word.Next;
            word.Processed = true;
            next.Processed = true;

            return string.Format(".Setup(f => f.{0})", next.GetFunction());
        }
    }

    public class Returns : IInlineGenerator
    {
        public const string Name = "returns";
        //.Returns(() => new ContractModel())
        public string Generate(Word word)
        {
            // not valid actually - error?
            if (word.Next == null)
                return null;

            var next = word.Next;
            word.Processed = true;
            next.Processed = true;

            return string.Format(".Returns(() => {0})", next.GetValue());
        }
    }

    public class Act : IInlineGenerator
    {
        public const string Name = "act";
        //.Returns(() => new ContractModel())
        public string Generate(Word word)
        {
            var classname = word.Dock.Blocks[0].Lines[0].Words[1].Text;
            var method = word.Block.Lines[0].Words[1].Text.Split('_')[0];
            var parameters = string.Join(",", word.GetParams());

            word.Processed = true;

            return string.Format("var actor = mock.Create<{0}>();\n\t\t\t\tactor.{1}({2})", classname, method, parameters);
        }
    }

    public class Verify : IInlineGenerator
    {
        public const string Name = "verify";
        //.Verify(f => f.Publish(It.IsAny<object>()), Times.Once)
        public string Generate(Word word)
        {
            if (word.Next == null)
                return null;

            var next = word.Next;
            var result = string.Format(".Verify(f => f.{0}", next.GetFunction());

            if (next.Next != null && Generator.InlineGenerators.ContainsKey(next.Next.GetPlain()))
            {
                result += ", " + Generator.InlineGenerators[next.Next.GetPlain()].Generate(next.Next);
            }
            result += ")";

            word.Processed = true;
            next.Processed = true;

            return result;
        }
    }

    public class Called : IInlineGenerator
    {
        public const string Name = "called";
        // Times.Once
        public string Generate(Word word)
        {
            if (word.Next == null)
                return null;

            var next = word.Next;
            string times;
            switch (next.GetPlain())
            {
                case "once":
                    times = "Once";
                    break;
                default:
                    times = string.Format("Exact({0})", next.Text);
                    break;
            }

            word.Processed = true;
            next.Processed = true;

            return string.Format("Times.{0}", times);
        }
    }
}