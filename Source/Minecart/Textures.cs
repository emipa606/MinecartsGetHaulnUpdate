using UnityEngine;
using Verse;


namespace Minecart
{
    [StaticConstructorOnStartup]
    static internal class Textures
    {
        public static Texture2D AutoSwitch_UI = ContentFinder<Texture2D>.Get("UI/AutoSwitch_ScreenIcon");
        public static Texture2D RailDirectionRight = ContentFinder<Texture2D>.Get("UI/RailDirectionRight");
        public static Texture2D RailDirectionLeft = ContentFinder<Texture2D>.Get("UI/RailDirectionLeft");
    }
}
