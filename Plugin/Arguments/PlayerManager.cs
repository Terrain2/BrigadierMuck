using System.Linq;
using System.Threading.Tasks;
using Brigadier.Plugin;
using Dawn;
using static Brigadier.StaticUtils;

namespace Brigadier.Arguments
{
    public class PlayerManagerArgument : ArgumentType<PlayerManager>
    {
        public override PlayerManager Parse(StringScanner scanner)
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

                        var friends = SteamLobby.Instance.currentLobby.Members.Where(friend => friend.Id == steamId);
                        if (friends.Count() == 1)
                        {
                            scanner.ParseStack.Pop();
                            return GameManager.players[SteamLobby.steamIdToClientId[friends.Single().Id]];
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

                        if (GameManager.players.TryGetValue(clientId, out var player))
                        {
                            scanner.ParseStack.Pop();
                            return player;
                        }
                        else
                        {
                            throw scanner.MakeException("Client Id was not found in the session");
                        }
                    }
                default:
                    foreach (var player in GameManager.players.Values)
                    {
                        if (scanner.Remaining.StartsWith(player.username))
                        {
                            scanner.Skip(player.username.Length);
                            scanner.ParseStack.Pop();
                            return player;
                        }
                    }
                    throw scanner.MakeException("Expected username");
            }
        }
        public override Task<Suggestions> ListSuggestions(CommandContext context, SuggestionsBuilder builder)
        {
            foreach (var player in GameManager.players.Values)
            {
                if (player.username.StartsWith(builder.RemainingLowercase))
                {
                    builder.Suggest(player.username);
                }

                var steamId = player.SteamId();
                if (builder.Remaining.StartsWith("@") && $"@{steamId}".StartsWith(builder.Remaining))
                {
                    builder.Suggest(steamId.Value, $"@{steamId}");
                }

                if (builder.Remaining.StartsWith("#") && $"#{player.id}".StartsWith(builder.Remaining))
                {
                    builder.Suggest(player.id, $"#{player.id}");
                }
            }
            return builder.BuildTask();
        }
    }
}