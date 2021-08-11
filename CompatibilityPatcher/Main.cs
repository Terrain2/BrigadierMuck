using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;

namespace BrigadierCompat
{
    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs
        {
            get
            {
                yield return "VW_COMMANDS.dll";
                yield return "ChatCommands.dll";
                if (patches == 0)
                {
                    log.LogError("ARE YOU TRYING TO USE iiVeil ChatCommands OR VALIDWARE COMMANDS WITH BRIGADIER?");
                    log.LogError("THE COMPATIBILITY PATCHER WAS LOADED BUT NEITHER PLUGIN WAS PATCHED");
                    log.LogError($"MAKE SURE TO ADD BepInEx{Path.DirectorySeparatorChar}plugins TO YOUR DOORSTOP PATH!");
                }
            }
        }

        private static readonly ManualLogSource log = Logger.CreateLogSource("BrigadierCompat");
        private static int patches = 0;

        public static void Patch(AssemblyDefinition assembly)
        {
            patches++;
            TypeDefinition type;
            if (assembly.Name.Name == "VW_COMMANDS") type = assembly.MainModule.GetType("VW_COMMANDS.Chat_Patches", "ChatPatch");
            else if (assembly.Name.Name == "ChatCommands") type = assembly.MainModule.GetType("ChatCommands.Patches", "ParseCommands");
            else return;
            var method = type.Methods.FirstOrDefault(
                m => m.ReturnType == m.Module.TypeSystem.Void
                && m.Name == "Postfix"
                && m.GenericParameters.Count == 0
                && m.Parameters.Count == 2
                && m.Parameters[0].Name == "__instance"
                && m.Parameters[0].ParameterType.FullName == "ChatBox"
                && m.Parameters[1].Name == "message"
                && m.Parameters[1].ParameterType == m.Module.TypeSystem.String
                && m.CustomAttributes.Any(attr => attr.AttributeType.FullName == "HarmonyLib.HarmonyPatch")
            );
            if (method == default) return;
            var patch = method.CustomAttributes.First(attr => attr.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
            if (patch.ConstructorArguments.Count == 2
            && patch.ConstructorArguments[1].Type == method.Module.TypeSystem.String
            && (string)patch.ConstructorArguments[1].Value == "SendMessage")
            {
                log.LogInfo($"fixed attribute on {method.FullName}");
                patch.ConstructorArguments[1] = new CustomAttributeArgument(method.Module.TypeSystem.String, "ChatCommand");
            }
        }
    }
}