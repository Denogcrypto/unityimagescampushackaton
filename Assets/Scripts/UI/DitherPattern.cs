using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Patrones de punteado tipo Bayer para señalar el nivel de impacto de una
/// actividad sin usar color (4.12, S9: la paleta es monocromática). Genera
/// un sprite 4×4 por ImpactTier, cacheado, pensado para un Image en modo
/// Tiled (repite el patrón sobre el área del ícono).
/// </summary>
public static class DitherPattern
{
    // 1 = punto opaco, 0 = hueco transparente. Densidad creciente por tier.
    private static readonly int[,] Bajo = { { 0, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 1, 0 } };
    private static readonly int[,] Medio = { { 1, 0, 1, 0 }, { 0, 1, 0, 0 }, { 1, 0, 1, 0 }, { 0, 0, 0, 1 } };
    private static readonly int[,] Alto = { { 1, 1, 1, 0 }, { 1, 1, 0, 1 }, { 1, 0, 1, 1 }, { 0, 1, 1, 1 } };

    private static readonly Dictionary<ImpactTier, Sprite> cache = new Dictionary<ImpactTier, Sprite>();

    public static Sprite Get(ImpactTier tier)
    {
        if (cache.TryGetValue(tier, out var cached)) return cached;

        var pattern = tier switch
        {
            ImpactTier.Bajo => Bajo,
            ImpactTier.Medio => Medio,
            ImpactTier.Alto => Alto,
            _ => Bajo,
        };

        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, pattern[y, x] == 1 ? Color.black : Color.clear);
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        cache[tier] = sprite;
        return sprite;
    }
}
