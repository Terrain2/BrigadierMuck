namespace Brigadier
{
    public class LiteralArgumentBuilder : ArgumentBuilder<LiteralArgumentBuilder>
    {
        protected override LiteralArgumentBuilder This => this;
        public string Literal { get; protected internal set; }
        public LiteralArgumentBuilder(string literal)
        {
            Literal = literal;
        }

        public override CommandNode Build()
        {
            var node = new LiteralCommandNode(Literal, Command, Requirement, Target, RedirectModifier, Forks);
            foreach (var argument in Arguments)
            {
                node.AddChild(argument);
            }
            return node;

        }
    }
}