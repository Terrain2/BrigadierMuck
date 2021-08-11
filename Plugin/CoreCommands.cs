using System.Linq;
using Steamworks;
using UnityEngine;

namespace Brigadier.Plugin
{
    using static StaticUtils;
    internal static class CoreCommands
    {
        public static void Register()
        {
            Dispatcher.Register(Literal("seed").ExecutesLocal(Seed));
            Dispatcher.Register(Literal("ping").ExecutesLocal(Ping));
            Dispatcher.Register(Literal("debug").ExecutesLocal(Debug));
            Dispatcher.Register(Literal("kill").ExecutesLocal(Kill));
            Dispatcher.Register(Literal("legacy")
                .Then(
                    Argument("command", Arguments.GreedyString())
                        .ExecutesLocal(Legacy)
                )
            );
            Dispatcher.Register(Literal("kick")
                .Requires(s => s.IsHost)
                .Then(
                    Argument("client", Arguments.Client())
                        .ExecutesLocal(Kick)
                )
            );

            Dispatcher.Register(Literal("help")
                .Then(
                    Argument("command", Arguments.GreedyString())
                        .ExecutesLocal(HelpSpecific)
                ).ExecutesLocal(HelpList)
            );
        }

        public static void Seed(CommandContext ctx)
        {
            var seed = GameManager.gameSettings.Seed;
            ctx.Sender.SendMessage($"{ChatColors.Cyan}Seed: {seed} (copied to clipboard){ChatColors.End}");
            GUIUtility.systemCopyBuffer = seed.ToString();
        }

        public static void Ping(CommandContext ctx)
        {
            ctx.Sender.SendMessage($"{ChatColors.Cyan}pong{ChatColors.End}");
        }

        public static void Debug(CommandContext ctx)
        {
            DebugNet.Instance.ToggleConsole();
        }

        public static void Kill(CommandContext ctx)
        {
            PlayerStatus.Instance.Damage(0, 0, true);
        }

        public static void Legacy(CommandContext ctx)
        {
            ChatBox.Instance.ChatCommand("/" + ctx.Argument<string>("command"));
        }

        public static void Kick(CommandContext ctx)
        {
            var client = ctx.Argument<Client>("client");
            ServerHandle.KickPlayer(client.id);
            SteamNetworking.CloseP2PSessionWithUser(client.player.steamId);
            ctx.Sender.SendMessage("Failed to kick player...");
        }

        static void HelpList(CommandContext ctx)
        {
            var usages = Dispatcher.GetSmartUsage(Dispatcher.RootNode, ctx.Sender);
            foreach (var usage in usages.Values)
            {
                ctx.Sender.SendMessage($"/{usage}");
            }
        }

        static void HelpSpecific(CommandContext ctx)
        {
            var parse = Dispatcher.Parse(ctx.Argument<string>("command"), ctx.Sender);
            if (!parse.Context.Nodes.Any()) throw new CommandSyntaxException("Unknown command or insufficient permissions");

            var usages = Dispatcher.GetSmartUsage(parse.Context.Nodes.Last().Node, ctx.Sender);
            foreach (var usage in usages.Values)
            {
                ctx.Sender.SendMessage($"/{parse.Scanner.Content} {usage}");
            }
        }
    }
}