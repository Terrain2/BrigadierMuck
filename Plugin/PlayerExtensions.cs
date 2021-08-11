using System.Linq;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace Brigadier.Plugin
{
    public static class PlayerExtensions
    {
        public static Quaternion Rotation(this Player player) => Quaternion.Euler(player.xOrientation, player.yOrientation, 0);
        public static SteamId SteamId(this PlayerManager manager) =>
            Server.clients?.GetValueSafe(manager.id)?.player.steamId
            ?? SteamLobby.Instance.currentLobby.Members.SingleOrDefault(friend => friend.Name == manager.username).Id;
    }
}