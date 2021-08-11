using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brigadier
{
    public class CommandDispatcher
    {
        public readonly RootCommandNode RootNode;
        public CommandDispatcher() : this(null) { }
        public CommandDispatcher(RootCommandNode root)
        {
            RootNode = root ?? new RootCommandNode();
        }

        public LiteralCommandNode Register(LiteralArgumentBuilder command)
        {
            var built = command.Build();
            RootNode.AddChild(built);
            return built as LiteralCommandNode;
        }

        public void Execute(string input, CommandSender sender) => Execute(new StringScanner(input), sender);
        public void Execute(StringScanner input, CommandSender sender) => Execute(Parse(input, sender));
        public void Execute(ParseResults parse)
        {
            if (parse.Scanner.CanRead())
            {
                if (parse.Exceptions.Count == 1) throw parse.Exceptions.Single().Value;
                if (parse.Context.Range.Start.Equals(parse.Context.Range.End)) throw new CommandSyntaxException("Unknown command", parse.Scanner.Content, parse.Scanner.Cursor);
                throw new CommandSyntaxException("Incorrect argument for command", parse.Scanner.Content, parse.Scanner.Cursor);
            }

            var forked = false;
            var foundCommand = false;
            var original = parse.Context.Build(parse.Scanner.Content);
            var contexts = new List<CommandContext> { original };
            var next = new List<CommandContext>();

            while (contexts.Any())
            {
                foreach (var context in contexts)
                {
                    if (context.Child is not null)
                    {
                        forked |= context.Forks;
                        if (context.Child.Nodes.Any())
                        {
                            foundCommand = true;
                            if (context.RedirectModifier == null)
                            {
                                next.Add(context.Child with { Sender = context.Sender });
                            }
                            else
                            {
                                try
                                {
                                    foreach (var sender in context.RedirectModifier(context)) next.Add(context.Child with { Sender = sender });
                                }
                                catch (CommandSyntaxException)
                                {
                                    if (!forked) throw;
                                }
                            }
                        }
                    }
                    else if (context.Command != null)
                    {
                        foundCommand = true;
                        try
                        {
                            context.Command(context);
                        }
                        catch (CommandSyntaxException)
                        {
                            if (!forked) throw;
                        }
                    }
                }
                contexts = next;
                next = new List<CommandContext>();
            }

            if (!foundCommand) throw new CommandSyntaxException("Unknown command", parse.Scanner.Content, parse.Scanner.Cursor);
        }

        public ParseResults Parse(string command, CommandSender sender) => Parse(new StringScanner(command), sender);
        public ParseResults Parse(StringScanner command, CommandSender sender) => ParseNodes(RootNode, command, new CommandContextBuilder(this, sender, RootNode, command.Cursor));
        private ParseResults ParseNodes(CommandNode node, StringScanner originalScanner, CommandContextBuilder contextSoFar)
        {
            var sender = contextSoFar.Sender;
            var errors = new Dictionary<CommandNode, CommandSyntaxException>();
            var potentials = new List<ParseResults>();

            foreach (var child in node.GetRelevantNodes(originalScanner))
            {
                if (!sender.CanUse(child)) continue;
                var context = contextSoFar.Copy();
                var scanner = new StringScanner(originalScanner);
                try
                {
                    try
                    {
                        child.Parse(scanner, context);
                    }
                    catch (CommandSyntaxException)
                    {
                        throw;
                    }
                    catch (FileNotFoundException ex) when (ex.FileName == "Dawn.ValueString, Version=2.1.0.0, Culture=neutral, PublicKeyToken=99ae35dcef5291a4")
                    {
                        throw new CommandSyntaxException(ex, "Missing dependency: Dawn.ValueString");
                    }
                    catch (Exception ex)
                    {
                        throw new CommandSyntaxException(ex, "Could not parse command", scanner.Content, scanner.Cursor);
                    }
                    if (scanner.CanRead() && !char.IsWhiteSpace(scanner.Next)) throw new CommandSyntaxException("Expected whitespace to end one argument, but found trailing data", scanner.Content, scanner.Cursor);
                }
                catch (CommandSyntaxException ex)
                {
                    errors[child] = ex;
                    scanner.Cursor = originalScanner.Cursor;
                    continue;
                }

                context.Command = child.Command;
                if (scanner.CanRead(child.Redirect == null ? 2 : 1))
                {
                    scanner.Skip();
                    if (child.Redirect != null)
                    {
                        var childContext = new CommandContextBuilder(this, sender, child.Redirect, scanner.Cursor);
                        var parse = ParseNodes(child.Redirect, scanner, childContext);
                        context.Child = parse.Context;
                        return parse with { Context = context };
                    }
                    else
                    {
                        potentials.Add(ParseNodes(child, scanner, context));
                    }
                }
                else
                {
                    potentials.Add(new ParseResults(context, scanner, new Dictionary<CommandNode, CommandSyntaxException>()));
                }
            }

            if (potentials.Any())
            {
                potentials.Sort((a, b) =>
                {
                    if (!a.Scanner.CanRead() && b.Scanner.CanRead()) return -1;
                    if (a.Scanner.CanRead() && !b.Scanner.CanRead()) return 1;
                    if (!a.Exceptions.Any() && b.Exceptions.Any()) return -1;
                    if (a.Exceptions.Any() && !b.Exceptions.Any()) return 1;
                    return 0;
                });
                return potentials.First();
            }

            return new ParseResults(contextSoFar, originalScanner, errors);
        }

        public List<string> GetAllUsage(CommandNode node, CommandSender sender, bool restricted)
        {
            var result = new List<string>();
            GetAllUsage(node, sender, result, "", restricted);
            return result;
        }

        private void GetAllUsage(CommandNode node, CommandSender sender, List<string> result, string prefix, bool restricted)
        {
            if (restricted && !sender.CanUse(node)) return;
            if (node.Command != null) result.Add(prefix);

            if (node.Redirect != null)
            {
                var redirect = node.Redirect == RootNode ? "..." : $"-> {node.Redirect.Usage}";
                result.Add(string.IsNullOrEmpty(prefix) ? $"{node.Usage} {redirect}" : $"{prefix} {redirect}");
            }
            else
            {
                foreach (var child in node.Children)
                {
                    GetAllUsage(child, sender, result, string.IsNullOrEmpty(prefix) ? child.Usage : $"{prefix} {child.Usage}", restricted);
                }
            }
        }

        public Dictionary<CommandNode, string> GetSmartUsage(CommandNode node, CommandSender sender)
        {
            var result = new Dictionary<CommandNode, string>();

            var optional = node.Command != null;
            foreach (var child in node.Children)
            {
                var usage = GetSmartUsage(child, sender, optional, false);
                if (usage != null)
                {
                    result[child] = usage;
                }
            }

            return result;
        }

        private string GetSmartUsage(CommandNode node, CommandSender sender, bool optional, bool deep)
        {
            if (!sender.CanUse(node)) return null;

            var self = optional ? $"[{node.Usage}]" : node.Usage;
            var childOptional = node.Command != null;
            var open = childOptional ? "[" : "(";
            var close = childOptional ? "]" : ")";

            if (!deep)
            {
                if (node.Redirect != null)
                {
                    var redirect = node.Redirect == RootNode ? "..." : $"-> {node.Redirect.Usage}";
                    return $"{self} {redirect}";
                }
                else
                {
                    var children = node.Children.Where(sender.CanUse);
                    if (children.Count() == 1)
                    {
                        var usage = GetSmartUsage(children.First(), sender, childOptional, childOptional);
                        if (usage != null) return $"{self} {usage}";
                    }
                    else if (children.Any())
                    {
                        var childUsage = new HashSet<string>();
                        foreach (var child in children)
                        {
                            var usage = GetSmartUsage(child, sender, childOptional, true);
                            if (usage != null) childUsage.Add(usage);
                        }
                        if (childUsage.Count == 1)
                        {
                            var usage = childUsage.First();
                            return childOptional ? $"{self} [{usage}]" : $"{self} {usage}";
                        }
                        else
                        {
                            var builder = new StringBuilder(open);
                            var count = 0;
                            foreach (var child in children)
                            {
                                if (count > 0)
                                {
                                    builder.Append('|');
                                }
                                builder.Append(child.Usage);
                                count++;
                            }
                            if (count > 0)
                            {
                                builder.Append(close);
                                return $"{self} {builder}";
                            }
                        }
                    }
                }
            }

            return self;
        }


        public CommandNode FindNode(params string[] path) => FindNode(path.AsEnumerable());

        public CommandNode FindNode(IEnumerable<string> path)
        {
            CommandNode node = RootNode;
            foreach (var name in path) node = node[name] ?? throw new KeyNotFoundException();
            return node;
        }

        public bool TryFindNode(out CommandNode node, params string[] path) => TryFindNode(path, out node);

        public bool TryFindNode(IEnumerable<string> path, out CommandNode node)
        {
            node = RootNode;
            foreach (var name in path) node = node?[name];
            return node != null;
        }

        public List<string> GetPath(CommandNode target)
        {
            var nodes = new List<List<CommandNode>>();
            AddPaths(RootNode, nodes, new List<CommandNode>());

            foreach (var list in nodes)
            {
                if (list.Last() == target)
                {
                    var result = new List<string>(list.Count);
                    foreach (var node in list)
                    {
                        if (node != RootNode) result.Add(node.Name);
                    }
                    return result;
                }
            }

            return new List<string> { };
        }

        private void AddPaths(CommandNode node, List<List<CommandNode>> result, List<CommandNode> parents)
        {
#pragma warning disable IDE0028 // invalid fix i believe
            var current = new List<CommandNode>(parents);
#pragma warning restore IDE0028
            current.Add(node);
            result.Add(current);

            foreach (var child in node.Children)
            {
                AddPaths(child, result, current);
            }
        }

        public Task<Suggestions> GetCompletionSuggestions(ParseResults parse) => GetCompletionSuggestions(parse, parse.Scanner.Length);

        public async Task<Suggestions> GetCompletionSuggestions(ParseResults parse, int cursor)
        {
            var nodeBeforeCursor = parse.Context.FindSuggestionContext(cursor);
            var parent = nodeBeforeCursor.Parent;
            var start = Math.Min(nodeBeforeCursor.Start.GetOffset(parse.Scanner.Length), cursor);

            var input = parse.Scanner.Content[..cursor];
            var tasks = parent.Children.Select(node =>
            {
                try
                {
                    return node.ListSuggestions(parse.Context.Build(input), new SuggestionsBuilder(input, start));
                }
                catch (CommandSyntaxException)
                {
                    return Suggestions.Empty;
                }
            });
            var suggestions = await Task.WhenAll(tasks);
            return Suggestions.Merge(parse.Scanner.Content, suggestions);
        }
    }
}