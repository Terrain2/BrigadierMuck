#if !TEST
using UnityEngine;
#endif
using Brigadier.Plugin;

namespace Brigadier
{
    public record CommandSender(
#if !TEST
        int Id,
        bool IsHost,
        bool CanCheat,
        bool IsLocal,
        Vector3 Position,
        Quaternion Rotation
#endif
    )
    {
        public bool CanUse(CommandNode node) => node.Requirement(this);
#if !TEST
        public void SendMessage(string message)
        {
            if (IsLocal)
            {
                ChatBox.Instance.AppendMessage(-1, message, "");
            }
            else
            {
                Packets.ServerSend.CommandFeedback(Id, message);
            }
        }
#endif
    }
}