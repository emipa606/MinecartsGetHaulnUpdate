using UnityEngine;
using Verse;

namespace Minecart;

[StaticConstructorOnStartup]
internal static class Textures
{
    public static readonly Texture2D AutoSwitch_UI = ContentFinder<Texture2D>.Get("UI/AutoSwitch_ScreenIcon");
    public static readonly Texture2D RailDirectionRight = ContentFinder<Texture2D>.Get("UI/RailDirectionRight");
    public static readonly Texture2D RailDirectionLeft = ContentFinder<Texture2D>.Get("UI/RailDirectionLeft");
}