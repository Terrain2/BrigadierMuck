using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BepInEx.Bootstrap;
using Terrain.Packets;
using UnityEngine;
using static Brigadier.StaticUtils;

namespace Brigadier.Plugin
{
    public class Packets
    {
        public class ServerSend
        {
            public static void CommandFeedback(int toClient, string message)
            {
                using var packet = Main.packets.WriteToClient(nameof(ClientHandle.CommandFeedback), toClient);
                packet.Write(message);
                packet.Send();
            }
        }

        public class ClientSend
        {
            public static void ExecuteOnServer(string command)
            {
                using var packet = Main.packets.WriteToServer(nameof(ServerHandle.ExecuteOnServer));
                packet.Write(command);
                packet.Send();
            }
        }

        public class ServerHandle
        {
            [OffroadPacket]
            public static void ExecuteOnServer(int fromClient, BinaryReader reader)
            {
                var command = reader.ReadString();
                var sender = new CommandSender(
                    Id: fromClient,
                    IsHost: fromClient == LocalClient.instance.myId,
                    CanCheat: Main.canCheat[fromClient],
                    IsLocal: false,
                    Position: Server.clients[fromClient].player.pos,
                    Rotation: Server.clients[fromClient].player.Rotation()
                );
                try
                {
                    Dispatcher.Execute(command, sender);
                }
                catch (CommandSyntaxException ex)
                {
                    if (ex.InnerException != null)
                    {
                        Debug.Log("The exception below occured while executing a server-side command.");
                        Debug.LogException(ex.InnerException);
                        sender.SendMessage(ChatColors.Error + "Inner stacktrace in server log output" + ChatColors.End);
                    }
                    sender.SendMessage(ChatColors.Error + ex.Message + ChatColors.End);
                    sender.SendMessage(ex.ContextAndRest);
                }
                catch (Exception ex)
                {
                    Debug.Log("The exception below occured while executing a server-side command.");
                    Debug.LogException(ex);
                    sender.SendMessage($"{ChatColors.Error}Unhandled {ex.GetType().FullName} (see server log output){ChatColors.End}");
                }
            }
        }

        static class ClientHandle
        {
            [OffroadPacket]
            public static void CommandFeedback(BinaryReader reader)
            {
                var message = reader.ReadString();
                ChatBox.Instance.AppendMessage(-1, message, "");
            }
        }
    }
}