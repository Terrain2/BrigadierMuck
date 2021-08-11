using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Brigadier.Plugin
{
    [HarmonyPatch(typeof(ChatBox))]
    static class Patches
    {
        [HarmonyPatch(nameof(ChatBox.Awake)), HarmonyPostfix]
        static void Awake(ChatBox __instance)
        {
            Main.CanLocalCheat = LocalClient.serverOwner;
            __instance.inputField.gameObject.AddComponent<ChatManager>();
        }

        [HarmonyPatch(nameof(ChatBox.UserInput)), HarmonyPrefix]
        static bool UserInput()
        {
            ChatManager.Instance.UserInput();
            return false;
        }

        [HarmonyPatch(nameof(ChatBox.SendMessage)), HarmonyPrefix]
        static bool SendMessage(string message)
        {
            ChatManager.Instance.SendChatMessage(message);
            return false;
        }

        [HarmonyPatch(nameof(ChatBox.ClearMessage)), HarmonyPostfix]
        static void ClearMessage(ChatBox __instance)
        {
            __instance.typing = false;
            __instance.CancelInvoke(nameof(ChatBox.HideChat));
            __instance.Invoke(nameof(ChatBox.HideChat), 5f);
            __instance.inputField.text = "";
            __instance.inputField.interactable = false;
        }

        [HarmonyPatch(typeof(TMP_InputField), "MoveUp", typeof(bool), typeof(bool)), HarmonyPrefix]
        static bool MoveUp(TMP_InputField __instance)
        {
            if (__instance == ChatManager.Instance?.inputField)
            {
                ChatManager.Instance.Move(1);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(TMP_InputField), "MoveDown", typeof(bool), typeof(bool)), HarmonyPrefix]
        static bool MoveDown(TMP_InputField __instance)
        {
            if (__instance == ChatManager.Instance?.inputField)
            {
                ChatManager.Instance.Move(-1);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Commands), nameof(Commands.Update)), HarmonyPrefix]
        static bool CommandsUpdate(Commands __instance)
        {
            MonoBehaviour.Destroy(__instance);
            return false;
        }
    }
}