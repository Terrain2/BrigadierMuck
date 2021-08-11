using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brigadier
{
    public abstract class CommandNode : IComparable<CommandNode>, IEquatable<CommandNode>
    {
        public abstract string Name { get; init; }
        public abstract string Usage { get; }

        private readonly Dictionary<string, CommandNode> children = new();
        private readonly Dictionary<string, LiteralCommandNode> literals = new();
        private readonly Dictionary<string, ArgumentCommandNode> arguments = new();

        public readonly Predicate<CommandSender> Requirement;
        public readonly CommandNode Redirect;
        public readonly RedirectModifier RedirectModifier;
        public readonly bool Forks;
        public Command Command { get; private set; }

        [Obsolete("This constructor only exists to allow mocking because DynamicProxy requires a parameterless constructor")]
        internal CommandNode() { }

        protected CommandNode(Command command, Predicate<CommandSender> requirement, CommandNode redirect, RedirectModifier modifier, bool forks)
        {
            Command = command;
            Requirement = requirement;
            Redirect = redirect;
            RedirectModifier = modifier;
            Forks = forks;
        }

        public IEnumerable<CommandNode> Children => children.Values;
        public CommandNode this[string name]
        {
            get
            {
                if (children.TryGetValue(name, out var result)) return result;
                return null;
            }
        }

        public int CompareTo(CommandNode other)
        {
            var a = this is LiteralCommandNode;
            var b = other is LiteralCommandNode;
            if (a == b) return Name.CompareTo(other.Name);

            return (other is LiteralCommandNode) ? 1 : -1;
        }

        public void AddChild(CommandNode node)
        {
            if (node is RootCommandNode) throw new InvalidOperationException("Cannot add a RootCommandNode as a child to any other CommandNode");
            if (children.TryGetValue(node.Name, out var child))
            {
                if (child.GetType() != node.GetType()) throw new InvalidOperationException($"Cannot add a {node.GetType().Name} '{node.Name}' because there already exists a {child.GetType().Name} with that name");
                if (node.Command != null) child.Command = node.Command;
                foreach (var grandchild in node.Children)
                {
                    child.AddChild(grandchild);
                }
            }
            else
            {
                children[node.Name] = node;
                if (node is LiteralCommandNode)
                {
                    literals[node.Name] = node as LiteralCommandNode;
                }
                else if (node is ArgumentCommandNode)
                {
                    arguments[node.Name] = node as ArgumentCommandNode;
                }
            }
        }

        protected abstract bool IsValidInput(string input);

        public override bool Equals(object other)
        {
            if (Object.ReferenceEquals(this, other)) return true;
            if (!(other is CommandNode)) return false;
            return Equals(other as CommandNode);
        }

        public bool Equals(CommandNode other)
        {
            if (!children.SequenceEqual(other.children)) return false;
            return Command == other.Command;
        }

        public override int GetHashCode() => (children, Command).GetHashCode();

        public static bool operator ==(CommandNode lhs, CommandNode rhs) => Equals(lhs, rhs);
        public static bool operator !=(CommandNode lhs, CommandNode rhs) => !Equals(lhs, rhs);

        public abstract void Parse(StringScanner scanner, CommandContextBuilder builder);
        public abstract Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder);
        public abstract BaseArgumentBuilder CreateBuilder();
        public IEnumerable<CommandNode> GetRelevantNodes(StringScanner input)
        {
            if (literals.Any())
            {
                var start = input.Cursor;
                input.SkipToWhitespace();
                var text = input.Content[start..input.Cursor];
                input.Cursor = start;
                if (literals.TryGetValue(text, out var literal))
                {
                    return new[] { literal };
                }
            }
            return arguments.Values;
        }
    }
}