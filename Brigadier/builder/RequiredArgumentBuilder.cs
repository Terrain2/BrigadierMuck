using Brigadier.Arguments;

namespace Brigadier
{
    public class RequiredArgumentBuilder<T> : ArgumentBuilder<RequiredArgumentBuilder<T>>
    {
        protected override RequiredArgumentBuilder<T> This => this;
        public readonly string Name;
        public readonly ArgumentType<T> type;
        public SuggestionProvider SuggestionsProvider;
        public RequiredArgumentBuilder(string name, ArgumentType<T> type)
        {
            Name = name;
            this.type = type;
        }

        public RequiredArgumentBuilder<T> Suggests(SuggestionProvider provider)
        {
            SuggestionsProvider = provider;
            return this;
        }

        public override CommandNode Build()
        {
            var node = new ArgumentCommandNode<T>(Name, type, Command, Requirement, Target, RedirectModifier, Forks, SuggestionsProvider);
            foreach (var argument in Arguments)
            {
                node.AddChild(argument);
            }
            return node;
        }
    }
}