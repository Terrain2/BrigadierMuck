using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Terrain.Packets;

namespace Brigadier.Plugin
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(gg1kommands_base.Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInIncompatibility(BetterChat.Main.Guid)] // Brigadier implements everything of BetterChat
    public class Main : BaseUnityPlugin
    {
        public const string
            Name = "Brigadier",
            Author = "Terrain",
            Guid = $"{Author}.{Name}",
            Version = "0.1.1.0";

        internal readonly ManualLogSource log;
        internal readonly Harmony harmony;
        internal readonly Assembly assembly;
        public readonly string modFolder;

        internal static bool CanLocalCheat = false;
        internal static OffroadPackets packets;
        internal static Dictionary<int, bool> canCheat;

        Main()
        {
            log = Logger;
            harmony = new Harmony(Guid);
            assembly = Assembly.GetExecutingAssembly();
            modFolder = Path.GetDirectoryName(assembly.Location);

#if !TEST
            CoreCommands.Register();
            packets = OffroadPackets.Create<Packets>();
            if (Chainloader.PluginInfos.ContainsKey(gg1kommands_base.Main.GUID)) gg1kommands.Register();
#endif

            harmony.PatchAll(assembly);

        }
    }
}