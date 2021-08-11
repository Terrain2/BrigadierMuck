#pragma warning disable IDE0090 // logerfo.csharp-colors does not recognize target-typed new colors
using System.Collections.Generic;
using UnityEngine;

namespace Brigadier.Plugin
{
    public static class Colors
    {
        public static readonly List<Color32> Arguments = new()
        {
            new Color32(0x55, 0xFF, 0xFF, 0xFF),
            new Color32(0xFF, 0xFF, 0x55, 0xFF),
            new Color32(0x55, 0xFF, 0x55, 0xFF),
            new Color32(0xFF, 0x55, 0xFF, 0xFF),
            new Color32(0xFF, 0xAA, 0x00, 0xFF),
        };
        public static readonly Color32 Error = new Color32(0xFF, 0x55, 0x55, 0xFF);
        public static readonly Color32 Literal = new Color32(0xAA, 0xAA, 0xAA, 0xFF);
        public static readonly Color32 SuggestionGhost = new Color32(0x88, 0x88, 0x88, 0xAA);
        public static readonly Color32 Plain = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        public static readonly Color32 Invisible = new Color32(0x00, 0x00, 0x00, 0x00);
    }
}