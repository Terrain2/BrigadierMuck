using Brigadier.Arguments;
namespace Brigadier
{
    public static partial class StaticUtils
    {
        public static partial class Arguments
        {
            public static ClientArgument Client() => new();
            public static PlayerManagerArgument PlayerManager() => new();
            public static PowerupArgument Powerup() => new();
            public static MobTypeArgument MobType() => new();
            public static Vector3Argument Vector3() => new();
            public static InventoryItemArgument InventoryItem() => new();
        }
    }
}