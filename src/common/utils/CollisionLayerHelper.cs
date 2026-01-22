using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenDesigner.Common.Utils
{
    public static class CollisionLayerHelper
    {
        public const int STATIC_WORLD = 1;
        public const int DYNAMIC_WORLD = 2;
        public const int PICKABLE_OBJECTS = 3;
        public const int WALL_WALKING = 4;
        public const int GRAPPLING_TARGET = 5;

        public const int ENVIRONMENT = 10;
        public const int TOOLS = 11;
        public const int KITCHEN_COMPONENTS = 12;

        public const int HELD_OBJECTS = 17;
        public const int PLAYER_HANDS = 18;
        public const int GRAB_HANDLES = 19;
        public const int PLAYER_BODY = 20;
        public const int POINTABLE_OBJECTS = 21;
        public const int HAND_POSE_AREAS = 22;
        public const int UI_OBJECTS = 23;

        public static uint SetLayer(uint currentMask, int layer, bool enabled)
        {
            if (layer < 1 || layer > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(layer), "Layer must be between 1 and 32.");
            }
            uint layerBit = 1u << (layer - 1);
            if (enabled)
            {
                return currentMask | layerBit;
            }
            else
            {
                return currentMask & ~layerBit;
            }
        }

    }
}
