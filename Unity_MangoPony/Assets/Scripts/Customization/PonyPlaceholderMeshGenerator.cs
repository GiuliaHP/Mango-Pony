using UnityEngine;
using MangoPony.Core;

namespace MangoPony.Customization
{
    /// <summary>
    /// Générateur de primitives et meshes proxy 3D pour le prototypage immédiat.
    /// Permet de tester le système modulaire, l'assemblage et les shaders
    /// même en l'absence de modèles FBX finaux.
    /// </summary>
    public static class PonyPlaceholderMeshGenerator
    {
        public static Mesh CreateBodyMesh(BodyType type)
        {
            Mesh mesh = new Mesh { name = $"Body_{type}" };
            Vector3 scale = Vector3.one;

            switch (type)
            {
                case BodyType.Trait:
                    scale = new Vector3(1.25f, 1.15f, 1.35f); // Robuste, musclé, épais
                    break;
                case BodyType.Arabe:
                    scale = new Vector3(0.85f, 1.05f, 0.95f); // Élancé, fin
                    break;
                case BodyType.Classique:
                default:
                    scale = new Vector3(1.0f, 1.0f, 1.1f);
                    break;
            }

            return GenerateCapsuleMesh(scale * 0.5f, 16, 8, mesh);
        }

        public static Mesh CreateHeadMesh(HeadType type)
        {
            Mesh mesh = new Mesh { name = $"Head_{type}" };
            Vector3 scale = new Vector3(0.35f, 0.45f, 0.65f);

            // Ajustement subtil de la forme selon le profil
            switch (type)
            {
                case HeadType.Bombee:
                    scale.y *= 1.15f; // Chanfrein busqué
                    break;
                case HeadType.Creuse:
                    scale.y *= 0.88f; // Chanfrein creux arabe
                    break;
                case HeadType.Plate:
                default:
                    break;
            }

            return GenerateTaperedBoxMesh(scale, mesh);
        }

        public static Mesh CreateHoofMesh(HoofType type)
        {
            Mesh mesh = new Mesh { name = $"Hoof_{type}" };
            Vector3 size = new Vector3(0.2f, 0.15f, 0.22f);

            switch (type)
            {
                case HoofType.Fendus:
                    size.x *= 1.1f;
                    break;
                case HoofType.Fanons:
                    size = new Vector3(0.28f, 0.25f, 0.3f); // Poils abondants
                    break;
                case HoofType.Classiques:
                default:
                    break;
            }

            return GenerateCylinderMesh(size.x, size.y, 12, mesh);
        }

        public static Mesh CreateManeMesh(ManeType type)
        {
            Mesh mesh = new Mesh { name = $"Mane_{type}" };
            Vector3 size = Vector3.one;

            switch (type)
            {
                case ManeType.Brosse:
                    size = new Vector3(0.12f, 0.35f, 0.8f);
                    break;
                case ManeType.Tresse:
                    size = new Vector3(0.15f, 0.25f, 0.85f);
                    break;
                case ManeType.Wavy:
                    size = new Vector3(0.25f, 0.55f, 0.9f);
                    break;
                case ManeType.Sauvage:
                    size = new Vector3(0.35f, 0.65f, 1.0f);
                    break;
                case ManeType.Classique:
                default:
                    size = new Vector3(0.18f, 0.45f, 0.85f);
                    break;
            }

            return GenerateCrestMesh(size, mesh);
        }

        public static Mesh CreateTailMesh(TailType type)
        {
            Mesh mesh = new Mesh { name = $"Tail_{type}" };
            Vector3 size = Vector3.one;

            switch (type)
            {
                case TailType.Licorne:
                    size = new Vector3(0.12f, 0.7f, 0.12f);
                    break;
                case TailType.Sirene:
                    size = new Vector3(0.45f, 0.8f, 0.1f);
                    break;
                case TailType.Tresses:
                    size = new Vector3(0.14f, 0.9f, 0.14f);
                    break;
                case TailType.Wavy:
                    size = new Vector3(0.3f, 0.95f, 0.25f);
                    break;
                case TailType.Classique:
                default:
                    size = new Vector3(0.22f, 0.9f, 0.22f);
                    break;
            }

            return GenerateConeMesh(size.x, size.y, 10, mesh);
        }

        public static Mesh CreateHornMesh(HornType type)
        {
            Mesh mesh = new Mesh { name = $"Horn_{type}" };

            switch (type)
            {
                case HornType.Droite:
                    return GenerateConeMesh(0.08f, 0.65f, 10, mesh);
                case HornType.Courbee:
                    return GenerateCurvedHornMesh(0.08f, 0.6f, -25f, mesh);
                case HornType.Rhino:
                    return GenerateConeMesh(0.14f, 0.35f, 8, mesh);
                case HornType.Chevre:
                    return GenerateCurvedHornMesh(0.07f, 0.5f, -40f, mesh);
                case HornType.Cerf:
                    return GenerateBranchMesh(mesh);
                case HornType.Bouc:
                    return GenerateCurvedHornMesh(0.09f, 0.55f, 45f, mesh);
                default:
                    return GenerateConeMesh(0.08f, 0.5f, 8, mesh);
            }
        }

        public static Mesh CreateWingMesh(WingType type)
        {
            Mesh mesh = new Mesh { name = $"Wing_{type}" };
            Vector2 size = new Vector2(0.8f, 0.5f);

            switch (type)
            {
                case WingType.Mini:
                    size = new Vector2(0.35f, 0.25f);
                    break;
                case WingType.Dragon:
                    size = new Vector2(1.1f, 0.65f);
                    break;
                case WingType.Plumes:
                default:
                    size = new Vector2(0.9f, 0.55f);
                    break;
            }

            return GenerateWingFanMesh(size, mesh);
        }

        #region Procedural Primitive Helpers

        private static Mesh GenerateCapsuleMesh(Vector3 scale, int longitude, int latitude, Mesh mesh)
        {
            int nbVertices = (latitude + 1) * (longitude + 1);
            Vector3[] vertices = new Vector3[nbVertices];
            Vector3[] normals = new Vector3[nbVertices];
            Vector2[] uvs = new Vector2[nbVertices];
            int[] triangles = new int[latitude * longitude * 6];

            int vertIdx = 0;
            for (int lat = 0; lat <= latitude; lat++)
            {
                float theta = lat * Mathf.PI / latitude;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= longitude; lon++)
                {
                    float phi = lon * 2f * Mathf.PI / longitude;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    Vector3 n = new Vector3(cosPhi * sinTheta, cosTheta, sinPhi * sinTheta);
                    normals[vertIdx] = n;
                    vertices[vertIdx] = Vector3.Scale(n, scale);
                    uvs[vertIdx] = new Vector2((float)lon / longitude, (float)lat / latitude);
                    vertIdx++;
                }
            }

            int triIdx = 0;
            for (int lat = 0; lat < latitude; lat++)
            {
                for (int lon = 0; lon < longitude; lon++)
                {
                    int current = lat * (longitude + 1) + lon;
                    int next = current + longitude + 1;

                    triangles[triIdx++] = current;
                    triangles[triIdx++] = next;
                    triangles[triIdx++] = current + 1;

                    triangles[triIdx++] = current + 1;
                    triangles[triIdx++] = next;
                    triangles[triIdx++] = next + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh GenerateTaperedBoxMesh(Vector3 size, Mesh mesh)
        {
            Vector3 s = size * 0.5f;
            Vector3[] v = new Vector3[]
            {
                // Devant
                new Vector3(-s.x * 0.7f, -s.y,  s.z),
                new Vector3( s.x * 0.7f, -s.y,  s.z),
                new Vector3( s.x * 0.7f,  s.y * 0.8f,  s.z),
                new Vector3(-s.x * 0.7f,  s.y * 0.8f,  s.z),
                // Derrière
                new Vector3(-s.x, -s.y, -s.z),
                new Vector3( s.x, -s.y, -s.z),
                new Vector3( s.x,  s.y, -s.z),
                new Vector3(-s.x,  s.y, -s.z),
            };

            int[] t = new int[]
            {
                0, 2, 1, 0, 3, 2, // Devant
                5, 6, 4, 4, 6, 7, // Arrière
                4, 7, 0, 0, 7, 3, // Gauche
                1, 2, 5, 5, 2, 6, // Droite
                3, 7, 2, 2, 7, 6, // Haut
                4, 0, 5, 5, 0, 1  // Bas
            };

            Vector2[] uvs = new Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                uvs[i] = new Vector2((v[i].x / size.x) + 0.5f, (v[i].z / size.z) + 0.5f);
            }

            mesh.vertices = v;
            mesh.triangles = t;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh GenerateCylinderMesh(float radius, float height, int segments, Mesh mesh)
        {
            Vector3[] v = new Vector3[(segments + 1) * 2 + 2];
            int[] t = new int[segments * 12];
            Vector2[] uvs = new Vector2[v.Length];

            float halfH = height * 0.5f;
            int vi = 0;

            for (int i = 0; i <= segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(a) * radius;
                float z = Mathf.Sin(a) * radius;

                v[vi] = new Vector3(x, -halfH, z);
                uvs[vi] = new Vector2((float)i / segments, 0f);
                vi++;

                v[vi] = new Vector3(x, halfH, z);
                uvs[vi] = new Vector2((float)i / segments, 1f);
                vi++;
            }

            // Centres top/bottom
            int bottomCenter = vi++;
            int topCenter = vi++;
            v[bottomCenter] = new Vector3(0, -halfH, 0);
            v[topCenter] = new Vector3(0, halfH, 0);
            uvs[bottomCenter] = new Vector2(0.5f, 0.5f);
            uvs[topCenter] = new Vector2(0.5f, 0.5f);

            int ti = 0;
            for (int i = 0; i < segments; i++)
            {
                int b0 = i * 2;
                int t0 = b0 + 1;
                int b1 = (i + 1) * 2;
                int t1 = b1 + 1;

                // Côtés
                t[ti++] = b0; t[ti++] = t0; t[ti++] = b1;
                t[ti++] = b1; t[ti++] = t0; t[ti++] = t1;

                // Bas
                t[ti++] = bottomCenter; t[ti++] = b0; t[ti++] = b1;
                // Haut
                t[ti++] = topCenter; t[ti++] = t1; t[ti++] = t0;
            }

            mesh.vertices = v;
            mesh.triangles = t;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh GenerateConeMesh(float radius, float height, int segments, Mesh mesh)
        {
            Vector3[] v = new Vector3[segments + 2];
            int[] t = new int[segments * 6];
            Vector2[] uvs = new Vector2[v.Length];

            v[0] = new Vector3(0, height, 0); // Pointe
            uvs[0] = new Vector2(0.5f, 1f);

            for (int i = 0; i < segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                v[i + 1] = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
                uvs[i + 1] = new Vector2((float)i / segments, 0f);
            }

            int baseCenter = segments + 1;
            v[baseCenter] = Vector3.zero;
            uvs[baseCenter] = new Vector2(0.5f, 0.5f);

            int ti = 0;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                // Côtés
                t[ti++] = 0;
                t[ti++] = i + 1;
                t[ti++] = next + 1;

                // Base
                t[ti++] = baseCenter;
                t[ti++] = next + 1;
                t[ti++] = i + 1;
            }

            mesh.vertices = v;
            mesh.triangles = t;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh GenerateCurvedHornMesh(float radius, float length, float curveAngle, Mesh mesh)
        {
            int segments = 8;
            int rings = 6;
            Vector3[] v = new Vector3[rings * segments + 1];
            Vector2[] uvs = new Vector2[v.Length];
            int[] t = new int[(rings - 1) * segments * 6 + segments * 3];

            for (int r = 0; r < rings; r++)
            {
                float frac = (float)r / (rings - 1);
                float currentRadius = radius * (1.0f - frac * 0.85f);
                float angle = frac * curveAngle * Mathf.Deg2Rad;

                Vector3 center = new Vector3(0, Mathf.Sin(Mathf.PI * 0.5f - angle) * length * frac, Mathf.Cos(Mathf.PI * 0.5f - angle) * length * frac);

                for (int s = 0; s < segments; s++)
                {
                    float a = (float)s / segments * Mathf.PI * 2f;
                    Vector3 offset = new Vector3(Mathf.Cos(a) * currentRadius, Mathf.Sin(a) * currentRadius, 0);
                    int idx = r * segments + s;
                    v[idx] = center + offset;
                    uvs[idx] = new Vector2((float)s / segments, frac);
                }
            }

            // Pointe
            int tipIdx = rings * segments;
            v[tipIdx] = v[(rings - 1) * segments] + Vector3.up * 0.05f;
            uvs[tipIdx] = new Vector2(0.5f, 1f);

            int ti = 0;
            for (int r = 0; r < rings - 1; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int next = (s + 1) % segments;
                    int curRow = r * segments;
                    int nextRow = (r + 1) * segments;

                    t[ti++] = curRow + s;
                    t[ti++] = nextRow + s;
                    t[ti++] = curRow + next;

                    t[ti++] = curRow + next;
                    t[ti++] = nextRow + s;
                    t[ti++] = nextRow + next;
                }
            }

            for (int s = 0; s < segments; s++)
            {
                int next = (s + 1) % segments;
                int topRow = (rings - 1) * segments;
                t[ti++] = tipIdx;
                t[ti++] = topRow + s;
                t[ti++] = topRow + next;
            }

            mesh.vertices = v;
            mesh.triangles = t;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh GenerateBranchMesh(Mesh mesh)
        {
            return GenerateConeMesh(0.06f, 0.6f, 8, mesh);
        }

        private static Mesh GenerateCrestMesh(Vector3 size, Mesh mesh)
        {
            Vector3[] v = new Vector3[10];
            v[0] = new Vector3(-size.x, 0, -size.z * 0.5f);
            v[1] = new Vector3( size.x, 0, -size.z * 0.5f);
            v[2] = new Vector3( size.x, 0,  size.z * 0.5f);
            v[3] = new Vector3(-size.x, 0,  size.z * 0.5f);

            v[4] = new Vector3(-size.x * 0.5f, size.y, -size.z * 0.3f);
            v[5] = new Vector3( size.x * 0.5f, size.y, -size.z * 0.3f);
            v[6] = new Vector3( size.x * 0.5f, size.y * 1.2f,  0f);
            v[7] = new Vector3(-size.x * 0.5f, size.y * 1.2f,  0f);
            v[8] = new Vector3( size.x * 0.5f, size.y,  size.z * 0.3f);
            v[9] = new Vector3(-size.x * 0.5f, size.y,  size.z * 0.3f);

            int[] t = new int[]
            {
                0, 4, 1, 1, 4, 5,
                1, 5, 2, 2, 5, 8,
                2, 8, 3, 3, 8, 9,
                3, 9, 0, 0, 9, 4,
                4, 7, 5, 5, 7, 6,
                6, 7, 8, 8, 7, 9
            };

            Vector2[] uvs = new Vector2[v.Length];
            for (int i = 0; i < v.Length; i++) uvs[i] = new Vector2(v[i].x + 0.5f, v[i].z + 0.5f);

            mesh.vertices = v;
            mesh.triangles = t;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh GenerateWingFanMesh(Vector2 size, Mesh mesh)
        {
            int segments = 6;
            Vector3[] v = new Vector3[segments + 2];
            Vector2[] uvs = new Vector2[v.Length];
            int[] t = new int[segments * 6]; // Double sided

            v[0] = Vector3.zero; // Base d'attache
            uvs[0] = new Vector2(0f, 0.5f);

            for (int i = 0; i <= segments; i++)
            {
                float frac = (float)i / segments;
                float angle = Mathf.Lerp(-20f, 65f, frac) * Mathf.Deg2Rad;
                float radius = size.x * (0.6f + Mathf.Sin(frac * Mathf.PI) * 0.4f);

                v[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, Mathf.Sin(frac * Mathf.PI) * 0.08f);
                uvs[i + 1] = new Vector2(frac, 1f);
            }

            int ti = 0;
            for (int i = 0; i < segments; i++)
            {
                // Face avant
                t[ti++] = 0;
                t[ti++] = i + 1;
                t[ti++] = i + 2;

                // Face arrière
                t[ti++] = 0;
                t[ti++] = i + 2;
                t[ti++] = i + 1;
            }

            mesh.vertices = v;
            mesh.triangles = t;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion
    }
}

