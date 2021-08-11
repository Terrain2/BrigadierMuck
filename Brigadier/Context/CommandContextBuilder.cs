using System;
using System.Collections.Generic;
using System.Linq;

namespace Brigadier
{
    public class CommandContextBuilder
    {
        public readonly Dictionary<string, ParsedArgument> Arguments = new();
        public readonly CommandNode RootNode;
        public readonly List<ParsedCommandNode> Nodes = new();
        public readonly CommandDispatcher Dispatcher;
        public CommandSender Sender;
        public Command Command;
        public CommandContextBuilder Child;
        public Range Range;
        public RedirectModifier RedirectModifier;
        public bool Forks;

        public CommandContextBuilder(CommandDispatcher dispatcher, CommandSender sender, CommandNode rootNode, Index start)
        {
            Dispatcher = dispatcher;
            Sender = sender;
            RootNode = rootNode;
            Range = start..start;
        }

        public void AddNode(CommandNode node, Range range)
        {
            Nodes.Add(new ParsedCommandNode(node, range));
            Range = new Range(Math.Min(Range.Start.Value, range.Start.Value), Math.Max(Range.End.Value, range.End.Value));
            RedirectModifier = node.RedirectModifier;
            Forks = node.Forks;
        }

        public CommandContextBuilder Copy()
        {
            var copy = new CommandContextBuilder(Dispatcher, Sender, RootNode, Range.Start);
            copy.Command = Command;
            Arguments.ToList().ForEach(pair => copy.Arguments.Add(pair.Key, pair.Value));
            Nodes.ForEach(copy.Nodes.Add);
            copy.Child = Child;
            copy.Range = Range;
            copy.Forks = Forks;
            return copy;
        }

        public CommandContext Build(string input) => new(Sender, input, Arguments, Command, RootNode, Nodes, Range, Child?.Build(input), RedirectModifier, Forks);

        public SuggestionContext FindSuggestionContext(int cursor)
        {
            if (Range.Start.Value <= cursor)
            {
                if (Range.End.Value < cursor)
                {
                    if (Child != null) return Child.FindSuggestionContext(cursor);
                    if (Nodes.Any())
                    {
                        var last = Nodes.Last();
                        return new SuggestionContext(last.Node, last.Range.End.Value + 1);
                    }
                    return new SuggestionContext(RootNode, Range.Start);
                }
                else
                {
                    var prev = RootNode;
                    foreach (var node in Nodes)
                    {
                        if (node.Range.Start.Value <= cursor && cursor <= node.Range.End.Value)
                        {
                            return new SuggestionContext(prev, node.Range.Start);
                        }
                        prev = node.Node;
                    }
                    if (prev == null) throw new InvalidOperationException("Can't find node before cursor");
                    return new SuggestionContext(prev, Range.Start);
                }
            }
            throw new InvalidOperationException("Can't find node before cursor");
        }
    }
}