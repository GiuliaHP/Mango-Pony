using UnityEngine;
using MangoPony.Core;

namespace MangoPony.Customization
{
    /// <summary>
    /// Générateur procédural de masques de textures (Face marks, Balzanes, Patterns, Nuances)
    /// pour le prototypage et tests immédiats sans dépendance à des assets 2D externes.
    /// </summary>
    public static class PonyProceduralTextureGenerator
    {
        private const int DefaultResolution = 512;

        public static Texture2D CreateFaceMarkTexture(FaceMarkType type, int res = DefaultResolution)
        {
            Texture2D tex = CreateEmptyMask(res, "Tex_FaceMark_" + type);
            if (type == FaceMarkType.None) return tex;

            Color[] pixels = tex.GetPixels();

            for (int y = 0; y < res; y++)
            {
                float v = (float)y / res;
                for (int x = 0; x < res; x++)
                {
                    float u = (float)x / res;
                    float mask = 0f;

                    switch (type)
                    {
                        case FaceMarkType.Etoile:
                            // Étoile au front (centre ~ 0.5, 0.75)
                            float dx = Mathf.Abs(u - 0.5f) * 8f;
                            float dy = Mathf.Abs(v - 0.75f) * 8f;
                            mask = Mathf.Clamp01(1f - (dx + dy + Mathf.Sqrt(dx * dx + dy * dy)));
                            break;

                        case FaceMarkType.Pelote:
                            // Pelote circulaire au front
                            float distCircle = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.75f));
                            mask = Mathf.Clamp01((0.08f - distCircle) * 20f);
                            break;

                        case FaceMarkType.ListeFine:
                            // Bande étroite le long du chanfrein
                            if (v >= 0.35f && v <= 0.85f)
                            {
                                float distX = Mathf.Abs(u - 0.5f);
                                mask = Mathf.Clamp01((0.025f - distX) * 40f);
                            }
                            break;

                        case FaceMarkType.ListeLarge:
                            // Bande large couvrant front et chanfrein
                            if (v >= 0.30f && v <= 0.88f)
                            {
                                float distX = Mathf.Abs(u - 0.5f);
                                mask = Mathf.Clamp01((0.08f - distX) * 25f);
                            }
                            break;

                        case FaceMarkType.EtoileListee:
                            // Étoile + fine bande
                            float starDx = Mathf.Abs(u - 0.5f) * 7f;
                            float starDy = Mathf.Abs(v - 0.80f) * 7f;
                            float star = Mathf.Clamp01(1f - (starDx + starDy));
                            float stripe = 0f;
                            if (v >= 0.35f && v <= 0.80f)
                            {
                                stripe = Mathf.Clamp01((0.025f - Mathf.Abs(u - 0.5f)) * 40f);
                            }
                            mask = Mathf.Max(star, stripe);
                            break;
                    }

                    int idx = y * res + x;
                    pixels[idx] = new Color(mask, mask, mask, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        public static Texture2D CreateLegMarkTexture(LegMarkType type, int res = DefaultResolution)
        {
            Texture2D tex = CreateEmptyMask(res, "Tex_LegMark_" + type);
            if (type == LegMarkType.None) return tex;

            Color[] pixels = tex.GetPixels();

            for (int y = 0; y < res; y++)
            {
                float v = (float)y / res;
                for (int x = 0; x < res; x++)
                {
                    float u = (float)x / res;
                    float mask = 0f;

                    switch (type)
                    {
                        case LegMarkType.Couronne:
                            // Fine bande juste au-dessus du sabot
                            mask = Mathf.Clamp01((0.10f - v) * 25f);
                            break;

                        case LegMarkType.PetiteBalzane:
                            // Bas de jambe
                            mask = Mathf.Clamp01((0.22f - v) * 20f);
                            break;

                        case LegMarkType.MoyenneBalzane:
                            // Mi-canon
                            mask = Mathf.Clamp01((0.38f - v) * 15f);
                            break;

                        case LegMarkType.GrandeBalzane:
                            // Sous le genou/jarret
                            mask = Mathf.Clamp01((0.58f - v) * 15f);
                            break;

                        case LegMarkType.Rayures:
                            // Rayures zébrées horizontales sur le bas de jambe
                            if (v < 0.45f)
                            {
                                float wave = Mathf.Sin(v * 60f);
                                mask = wave > 0.1f ? 1f : 0f;
                            }
                            break;
                    }

                    int idx = y * res + x;
                    pixels[idx] = new Color(mask, mask, mask, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        public static Texture2D CreateBodyPatternTexture(BodyPatternType type, int res = DefaultResolution)
        {
            Texture2D tex = CreateEmptyMask(res, "Tex_Pattern_" + type);
            if (type == BodyPatternType.None) return tex;

            Color[] pixels = tex.GetPixels();

            for (int y = 0; y < res; y++)
            {
                float v = (float)y / res;
                for (int x = 0; x < res; x++)
                {
                    float u = (float)x / res;
                    float mask = 0f;

                    switch (type)
                    {
                        case BodyPatternType.Appaloosa:
                            // Taches léopard sur l'arrière-main
                            float noiseA = Mathf.PerlinNoise(u * 14f, v * 14f);
                            float noiseB = Mathf.PerlinNoise(u * 28f + 5f, v * 28f + 3f);
                            float combined = (noiseA * 0.7f + noiseB * 0.3f);
                            // Concentré sur la croupe
                            float croupZone = Mathf.Clamp01((u - 0.3f) * 2.5f);
                            mask = (combined > 0.58f ? 1f : 0f) * croupZone;
                            break;

                        case BodyPatternType.Cape:
                            // Couverture blanche uniforme sur l'arrière-train
                            float edge = Mathf.PerlinNoise(u * 6f, v * 6f) * 0.08f;
                            mask = Mathf.Clamp01((u - (0.45f + edge)) * 20f);
                            break;

                        case BodyPatternType.Tobiano:
                            // Grandes taches pie organiques
                            float pieNoise = Mathf.PerlinNoise(u * 5f, v * 5f) + Mathf.PerlinNoise(u * 10f + 12f, v * 10f + 7f) * 0.3f;
                            mask = Mathf.Clamp01((pieNoise - 0.65f) * 15f);
                            break;

                        case BodyPatternType.MiniTobiano:
                            // Taches pie plus petites et localisées
                            float miniNoise = Mathf.PerlinNoise(u * 8f + 30f, v * 8f + 40f);
                            mask = Mathf.Clamp01((miniNoise - 0.62f) * 20f);
                            break;
                    }

                    int idx = y * res + x;
                    pixels[idx] = new Color(mask, mask, mask, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        public static Texture2D CreateCoatNuanceTexture(CoatNuanceType type, int res = DefaultResolution)
        {
            Texture2D tex = CreateEmptyMask(res, "Tex_Nuance_" + type);
            if (type == CoatNuanceType.None) return tex;

            Color[] pixels = tex.GetPixels();

            for (int y = 0; y < res; y++)
            {
                float v = (float)y / res;
                for (int x = 0; x < res; x++)
                {
                    float u = (float)x / res;
                    float mask = 0f;

                    switch (type)
                    {
                        case CoatNuanceType.Pommele:
                            // Pommelures (anneaux plus clairs)
                            float cellU = (u * 16f) % 1.0f;
                            float cellV = (v * 16f) % 1.0f;
                            float dist = Vector2.Distance(new Vector2(cellU, cellV), new Vector2(0.5f, 0.5f));
                            // Anneau
                            float ring = Mathf.Exp(-Mathf.Pow((dist - 0.28f) * 10f, 2f));
                            float flankNoise = Mathf.PerlinNoise(u * 3f, v * 3f);
                            mask = ring * 0.65f * flankNoise;
                            break;

                        case CoatNuanceType.Pangare:
                            // Éclaircissement du ventre, du museau et de l'intérieur des cuisses
                            float bellyGradient = Mathf.Clamp01((0.45f - v) * 3f);
                            float muzzle = Mathf.Clamp01((0.15f - Vector2.Distance(new Vector2(u, v), new Vector2(0.12f, 0.85f))) * 8f);
                            mask = Mathf.Max(bellyGradient * 0.7f, muzzle);
                            break;

                        case CoatNuanceType.Isabelle:
                            // Ligne de mulet dorsale sombre + dégradé doux
                            float spineDist = Mathf.Abs(v - 0.78f);
                            float dorsalStripe = Mathf.Clamp01((0.03f - spineDist) * 30f);
                            mask = dorsalStripe * 0.8f;
                            break;
                    }

                    int idx = y * res + x;
                    pixels[idx] = new Color(mask, mask, mask, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        private static Texture2D CreateEmptyMask(int res, string name)
        {
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color[] clear = new Color[res * res];
            for (int i = 0; i < clear.Length; i++)
            {
                clear[i] = new Color(0f, 0f, 0f, 1f);
            }
            tex.SetPixels(clear);
            tex.Apply(false);
            return tex;
        }
    }
}

