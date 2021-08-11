using System;
using System.Text.RegularExpressions;
using UnityEngine;
using static Brigadier.StaticUtils;

namespace Brigadier.Plugin
{
    partial class ChatManager
    {
        Color32[] charColors = null;
        int cursor = -1;
        ParseResults lastParse;
        static readonly Regex newline = new("\n");
        void DispatchAwake()
        {
            ChatBox.Instance.inputField.richText = false;
            ChatBox.Instance.inputField.textComponent.OnPreRenderText += info =>
            {
                try
                {
                    // if this is called below TMP_InputField.UpdateLabel() and throws
                    // the exception is not caught and TMP_InputField.m_PreventCallback is never unset
                    // so it's locked forever, WTF?
                    ApplyColors(ChatBox.Instance.inputField.text, info);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            };
            ChatBox.Instance.inputField.onValueChanged.AddListener(text =>
            {
                if (newline.IsMatch(text))
                {
                    ChatBox.Instance.inputField.text = newline.Replace(text, "");
                }
                else
                {
                    ProcessColors(ChatBox.Instance);
                }
            });
        }

        public void SendChatMessage(string message)
        {
            ChatBox.Instance.ClearMessage();
            var scanner = new StringScanner(message);
            if (!scanner.CanRead())
            {
                return;
            }
            if (history.Count == 0 || history[0] != message) history.Insert(0, message);
            if (scanner.Read() == '/')
            {
                try
                {
                    Dispatcher.Execute(scanner, new CommandSender(
                        Id: LocalClient.instance.myId,
                        IsHost: LocalClient.serverOwner,
                        CanCheat: Main.CanLocalCheat,
                        IsLocal: true,
                        Position: PlayerMovement.Instance.rb.position,
                        Rotation: PlayerMovement.Instance.playerCam.rotation
                    ));
                }
                catch (CommandSyntaxException ex)
                {
                    if (ex.InnerException != null)
                    {
                        Debug.LogException(ex.InnerException);
                        ChatBox.Instance.AppendMessage(-1, ChatColors.Error + "Inner stacktrace is in log output" + ChatColors.End, "");
                    }
                    ChatBox.Instance.AppendMessage(-1, ChatColors.Error + ex.Message + ChatColors.End, "");
                    ChatBox.Instance.AppendMessage(-1, ex.ContextAndRest, "");
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    ChatBox.Instance.AppendMessage(-1, $"{ChatColors.Error}Unhandled {ex.GetType().FullName} (see log output){ChatColors.End}", "");
                }
            }
            else
            {
                ChatBox.Instance.AppendMessage(LocalClient.instance.myId, message, GameManager.players[LocalClient.instance.myId].username);
                ClientSend.SendChatMessage(message);
            }
        }
    }
}