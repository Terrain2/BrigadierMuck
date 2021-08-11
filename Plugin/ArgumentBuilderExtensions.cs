using Brigadier.Plugin;

namespace Brigadier
{
#if !TEST
    public static class ArgumentBuilderExtensions
    {
        public static T ExecutesLocal<T>(this T builder, Command command) where T : ArgumentBuilder<T>
        {
            builder.Command = ctx =>
            {
                if (ctx.Sender.IsLocal)
                {
                    command(ctx);
                }
                else
                {
                    ctx.Sender.SendMessage($"{ChatColors.Error}[Brigadier]: The client thinks the command should run on the server, but the server thinks it should run on the client.{ChatColors.End}");
                }
            };
            return builder;
        }

        public static T ExecutesServer<T>(this T builder, Command command) where T : ArgumentBuilder<T>
        {
            builder.Command = ctx =>
            {
                if (!ctx.Sender.IsLocal)
                {
                    command(ctx);
                }
                else
                {
                    Packets.ClientSend.ExecuteOnServer(ctx.Input[1..]); // remove slash from start
                }
            };
            return builder;
        }
    }
#endif
}