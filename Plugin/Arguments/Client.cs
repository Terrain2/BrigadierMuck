using System.Threading.Tasks;
using Dawn;
using static Brigadier.StaticUtils;

namespace Brigadier.Arguments
{
    public class ClientArgument : ArgumentType<Client>
    {
        public override Client Parse(StringScanner scanner)
        {
            scanner.ParseStack.Push(scanner.Cursor);

            if (!scanner.CanRead()) throw scanner.MakeException("Expected username, Client Id, or SteamId");

            switch (scanner.Next)
            {
                case '@':
                    {
                        scanner.Skip();
                        var input = scanner.MatchRegex(IntegerRegex);

                        if (string.IsNullOrEmpty(input)) throw scanner.MakeException($"Expected SteamId");
                        if (!ValueString.Of(input).Is(out ulong steamId)) throw scanner.MakeException($"Invalid SteamId");

                        if (SteamLobby.steamIdToClientId.TryGetValue(steamId, out var clientId))
                        {
                            scanner.ParseStack.Pop();
                            return Server.clients[clientId];
                        }
                        else
                        {
                            throw scanner.MakeException("SteamId was not found in the session");
                        }
                    }
                case '#':
                    {

                        scanner.Skip();
                        var input = scanner.MatchRegex(IntegerRegex);

                        if (string.IsNullOrEmpty(input)) throw scanner.MakeException($"Expected Client Id");
                        if (!ValueString.Of(input).Is(out int clientId)) throw scanner.MakeException($"Invalid Client Id");

                        if (Server.clients.TryGetValue(clientId, out var client) && client?.player != null && client.inLobby)
                        {
                            scanner.ParseStack.Pop();
                            return client;
                        }
                        else
                        {
                            throw scanner.MakeException("Client Id was not found in the session");
                        }
                    }
                default:
                    foreach (var client in Server.clients.Values)
                    {
                        if (scanner.Remaining.StartsWith(client.player.username))
                        {
                            scanner.Skip(client.player.username.Length);
                            scanner.ParseStack.Pop();
                            return client;
                        }
                    }
                    throw scanner.MakeException("Expected username");
            }
        }
        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder)
        {
            foreach (var client in Server.clients.Values)
            {
                if (client?.player == null || !client.inLobby) continue;
                if (client.player.username.ToLower().StartsWith(builder.RemainingLowercase))
                {
                    builder.Suggest(client.player.username);
                }

                if (builder.Remaining.StartsWith("@") && $"@{client.player.steamId}".StartsWith(builder.Remaining))
                {
                    builder.Suggest(client.player.steamId.Value, $"@{client.player.steamId}");
                }

                if (builder.Remaining.StartsWith("#") && $"#{client.player.id}".StartsWith(builder.Remaining))
                {
                    builder.Suggest(client.player.id, $"#{client.player.id}");
                }
            }
            return builder.BuildTask();
        }
    }
}