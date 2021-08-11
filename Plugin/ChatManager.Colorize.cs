using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Brigadier.StaticUtils;

namespace Brigadier.Plugin
{
    partial class ChatManager
    {
        void ProcessColors(ChatBox __instance)
        {
            cursor = -1;
            suggestions = null;
            selectedSuggestion = 0;
            topSuggestion = 0;
            suggestionText = "";
            suggestionsTextObj.text = "";
            suggestionsBackground.sizeDelta = new(0, 0);
            var scanner = new StringScanner(__instance.inputField.text);
            if (scanner.CanRead() && scanner.Read() == '/')
            {
                var parse = Dispatcher.Parse(scanner, new CommandSender(
                        Id: LocalClient.instance.myId,
                        IsHost: LocalClient.serverOwner,
                        CanCheat: Main.CanLocalCheat,
                        IsLocal: true,
                        Position: PlayerMovement.Instance.rb.position,
                        Rotation: PlayerMovement.Instance.playerCam.rotation
                    ));
                lastParse = parse;
                charColors = Enumerable.Repeat(Colors.Literal, parse.Scanner.Length).ToArray();
                ArgumentColors(parse);
            }
            else
            {
                charColors = null;
                lastParse = null;
            }
            __instance.inputField.textComponent.ForceMeshUpdate(forceTextReparsing: true);
        }

        void ArgumentColors(ParseResults parse)
        {
            var context = parse.Context;
            IEnumerable<ParsedCommandNode> nodes = new List<ParsedCommandNode>();
            while (context != null)
            {
                nodes = nodes.Concat(context.Nodes);
                context = context.Child;
            }
            var i = 0;
            foreach (var parsed in nodes)
            {
                if (parsed.Node is LiteralCommandNode) continue;
                var (start, end) = parsed.Range.Normalized(parse.Scanner.Length);
                for (var j = start; j < end; j++)
                {
                    charColors[j] = Colors.Arguments[i];
                }
                i++;
                i %= Colors.Arguments.Count;
            }
            if (parse.Scanner.CanRead())
            {
                var startEx = Math.Min(parse.Scanner.Cursor,
                    parse.Exceptions.Count == 1
                        ? parse.Exceptions.Single().Value.Cursor
                        : int.MaxValue);
                for (var j = startEx; j < charColors.Length; j++)
                {
                    charColors[j] = Colors.Error;
                }
            }
        }

        void ApplyColors(string text, TMP_TextInfo info)
        {
            if (charColors != null)
            {
                var i = 0;
                for (var j = 0; j < info.characterCount; j++)
                {
                    var charInfo = info.characterInfo[j];
                    Color32 color;
                    if (i >= text.Length + suggestionText.Length)
                    {
                        continue;
                    }
                    else if (i >= text.Length)
                    {
                        if (charInfo.character != suggestionText[i - text.Length]) continue;
                        color = Colors.SuggestionGhost;
                        i++;
                    }
                    else if (charInfo.character == text[i])
                    {
                        color = charColors[i++];
                    }
                    else
                    {
                        continue;
                    }
                    if (char.IsWhiteSpace(charInfo.character)) continue;
                    var vertices = info.meshInfo[charInfo.materialReferenceIndex].colors32;
                    for (var k = 0; k < 4; k++)
                    {
                        vertices[charInfo.vertexIndex + k] = color;
                    }
                }
            }
            else
            {
                for (var i = 0; i < info.characterCount; i++)
                {
                    var charInfo = info.characterInfo[i];
                    if (char.IsWhiteSpace(charInfo.character)) continue;
                    var vertices = info.meshInfo[charInfo.materialReferenceIndex].colors32;
                    for (var k = 0; k < 4; k++)
                    {
                        vertices[charInfo.vertexIndex + k] = Colors.Plain;
                    }
                }
            }
            info.textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }
}