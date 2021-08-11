using System;
using System.Linq;

namespace Brigadier.Plugin
{
    using static StaticUtils;
#pragma warning disable IDE1006
    class gg1kommands
#pragma warning restore IDE1006
    {
        public static void Register()
        {
            gg1kommands_base.Command.addedCommand += AddToTree;

            // just in case another plugin registered a command before brigadier loaded
            foreach (var command in gg1kommands_base.Command.commands)
            {
                AddToTree(command);
            }
        }

        static void AddToTree(gg1kommands_base.Command command)
        {
            // make sure we wanna register this
            if (!command.aliases.Any()) return;
            if (command.method is null) return;
            if (command.lowerArgBound > command.upperArgBound) return;
            if (command.lowerArgBound < 0 || command.upperArgBound < 0) return;

            // get data in nice format
            var name = command.aliases.First();
            var aliases = command.aliases.Skip(1);

            // make argument nodes
            var top = Literal(name);
            var args = new RequiredArgumentBuilder<string>[command.upperArgBound];
            for (var i = 0; i < command.upperArgBound; i++)
            {
                args[i] = Argument($"gg1karg_{i}", Arguments.String());
            }

            // register execute points, edge case for argless
            if (command.lowerArgBound == 0)
            {
                top.ExecutesLocal(ConvertExecute(command.method, 0));
                for (var i = 0; i < command.upperArgBound; i++)
                {
                    args[i].ExecutesLocal(ConvertExecute(command.method, i + 1));
                }
            }
            else
            {
                for (var i = command.lowerArgBound - 1; i < command.upperArgBound; i++)
                {
                    args[i].ExecutesLocal(ConvertExecute(command.method, i + 1));
                }
            }

            // chain the args in a single tree
            for (var i = 1; i < args.Length; i++) args[i - 1].Then(args[i]);
            if (args.Any()) top.Then(args.First());

            var node = Dispatcher.Register(top);

            // register aliases as well
            foreach (var alias in aliases)
            {
                Dispatcher.Register(Literal(alias).Redirect(node));
            }
        }

        static Command ConvertExecute(Action<string[]> method, int argCount)
        {
            return ctx =>
            {
                var args = new string[argCount + 1];
                for (var i = 0; i < argCount; i++)
                {
                    args[i + 1] = ctx.Argument<string>($"gg1karg_{i}");
                }
                args[0] = ctx.Nodes[0].Node.Name;
                method(args);
            };
        }
    }
}