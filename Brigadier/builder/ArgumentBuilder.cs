using System;
using System.Collections.Generic;
using System.Linq;

namespace Brigadier
{
    public abstract class BaseArgumentBuilder
    {
        internal readonly RootCommandNode arguments = new();
        public IEnumerable<CommandNode> Arguments => arguments.Children;
        public Command Command;
        public Predicate<CommandSender> Requirement = s => true;
        public CommandNode Target { get; internal set; }
        public RedirectModifier RedirectModifier { get; internal set; }
        public bool Forks { get; internal set; }

        public abstract CommandNode Build();
    }

    public abstract class ArgumentBuilder<T> : BaseArgumentBuilder where T : ArgumentBuilder<T>
    {
        protected abstract T This { get; }
        public T Then(BaseArgumentBuilder argument)
        {
            if (this.Target != null) throw new InvalidOperationException("Cannot add children to a redirected node");
            this.arguments.AddChild(argument.Build());
            return This;
        }

        public T Then(CommandNode node)
        {
            if (this.Target != null) throw new InvalidOperationException("Cannot add children to a redirected node");
            this.arguments.AddChild(node);
            return This;
        }

        public T Requires(Predicate<CommandSender> requirement)
        {
            this.Requirement = requirement;
            return This;
        }

        public T Redirect(CommandNode target) => this.Forward(target, null, false);

        public T Redirect(CommandNode target, SingleRedirectModifier modifier) => this.Forward(target, c => new[] { modifier(c) }, false);

        public T Fork(CommandNode target, RedirectModifier modifier) => this.Forward(target, modifier, true);

        public T Forward(CommandNode target, RedirectModifier modifier, bool fork)
        {
            if (this.arguments.Children.Any()) throw new InvalidOperationException("Cannot forward a node with children");
            this.Target = target;
            this.RedirectModifier = modifier;
            this.Forks = fork;
            return This;
        }

        public T ExecutesRaw(Command command)
        {
            this.Command = command;
            return This;
        }
    }
}