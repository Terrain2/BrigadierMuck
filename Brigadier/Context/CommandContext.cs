using System;
using System.Collections.Generic;
using System.Linq;
using Brigadier.Arguments;

namespace Brigadier
{
    public record CommandContext(
        CommandSender Sender,
        string Input,
        Dictionary<string, ParsedArgument> Arguments,
        Command Command,
        CommandNode RootNode,
        List<ParsedCommandNode> Nodes,
        Range Range,
        CommandContext Child,
        RedirectModifier RedirectModifier,
        bool Forks)
    {
        public V Argument<V>(string name)
        {
            if (Arguments.TryGetValue(name, out var argument))
            {
                return argument switch
                {
                    ParsedArgument<V> simple => simple.Result,
                    ParsedArgument<FinalizingArgument<V>> finalizing => finalizing.Result.Finalize(Sender),
                    _ => throw new ArgumentException($"Argument '{name}' is defined as {argument.GetType().Name}, not {typeof(V).Name}"),
                };
            }
            else
            {
                throw new ArgumentException($"No such argument '{name}' exists on this command");
            }
        }

        public virtual bool Equals(CommandContext other)
        {
            if (!Arguments.SequenceEqual(other.Arguments)) return false;
            if (!Nodes.SequenceEqual(other.Nodes)) return false;
            if (RootNode != other.RootNode) return false;
            if (Command != other.Command) return false;
            if (Sender != other.Sender) return false;
            if (Child != other.Child) return false;

            return true;
        }

        public override int GetHashCode() => (Sender, Arguments, Command, RootNode, Nodes, Child).GetHashCode();
    }
}