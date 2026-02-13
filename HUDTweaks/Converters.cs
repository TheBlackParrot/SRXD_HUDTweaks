using UnityEngine;

namespace HUDTweaks;

internal static class Converters
{
    public static Color ToColor(this Vector3 vec)
    {
        return new Color(vec.x, vec.y, vec.z, 1f);
    }
}

