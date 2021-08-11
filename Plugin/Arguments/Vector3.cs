using System.Threading.Tasks;
using Dawn;
using UnityEngine;
using static Brigadier.StaticUtils;

namespace Brigadier.Arguments
{
    public class Vector3Argument : ArgumentType<FinalizingArgument<Vector3>>
    {
        public override FinalizingArgument<Vector3> Parse(StringScanner scanner)
        {
            var values = new (float value, bool relative)[3];
            for (var i = 0; i < 3; i++)
            {
                scanner.SkipWhitespace();
                var relative = false;
                if (scanner.CanRead() && scanner.Next == '~')
                {
                    scanner.Skip();
                    if (!scanner.CanRead() || char.IsWhiteSpace(scanner.Next))
                    {
                        values[i] = (0, true);
                        continue;
                    }
                    relative = true;
                }
                values[i] = (scanner.MatchParse<float>(NumberRegex), relative);
            }

            return new FinalizingVector3(values[0], values[1], values[2]);
        }

        public class FinalizingVector3 : FinalizingArgument<Vector3>
        {
            public readonly bool xIsRelative;
            public readonly bool yIsRelative;
            public readonly bool zIsRelative;
            public readonly float x;
            public readonly float y;
            public readonly float z;

            public FinalizingVector3((float value, bool relative) x, (float value, bool relative) y, (float value, bool relative) z)
            {
                this.x = x.value;
                this.y = y.value;
                this.z = z.value;
                xIsRelative = x.relative;
                yIsRelative = y.relative;
                zIsRelative = z.relative;
            }

            public override Vector3 Finalize(CommandSender sender)
            {
                var result = new Vector3(x, y, z);
                if (xIsRelative) result.x += sender.Position.x;
                if (yIsRelative) result.y += sender.Position.y;
                if (zIsRelative) result.z += sender.Position.z;
                return result;
            }
        }
    }
}