#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MangoPony.Core;
using MangoPony.Customization;
using MangoPony.Shaders;
using MangoPony.CameraControl;
using MangoPony.UI;

namespace MangoPony.Editor
{
    /// <summary>
    /// Assistant de configuration automatisé pour le prototype Mango Pony.
    /// Génère les textures de masques procédurales, les meshes 3D proxy, les prefabs modulaires,
    /// la base de données ScriptableObject, ainsi que la scène de test interactive complète.
    /// </summary>
    public static class PonyCustomizerSetupWizard
    {
        private const string ArtRoot = "Assets/Art";
        private const string MeshesPath = "Assets/Art/Meshes";
        private const string PrefabsPath = "Assets/Art/Prefabs";
        private const string TexturesPath = "Assets/Art/Textures/Masks";
        private const string MaterialsPath = "Assets/Art/Materials";
        private const string DatabasePath = "Assets/ScriptableObjects/PonyCustomizationDatabase.asset";
        private const string ScenePath = "Assets/Scenes/MangoPonyCustomizer.unity";

        [MenuItem("Mango Pony/1. Setup Complet (Assets, Database & Scène)", false, 1)]
        public static void RunFullSetup()
        {
            EnsureDirectories();
            Material masterMat = CreateOrGetMasterMaterial();
            GenerateMaskTextures();
            GenerateProxyMeshesAndPrefabs(masterMat);
            PonyCustomizationDatabase db = CreateOrUpdateDatabase();
            CreateCustomizerScene(db);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Mango Pony", 
                "Configuration terminée avec succès !\n\n- Masques de textures générés\n- Meshes & Prefabs créés\n- Database ScriptableObject configurée\n- Scène 'MangoPonyCustomizer' prête et chargée dans la hiérarchie !", 
                "Génial !");
        }

        private static void EnsureDirectories()
        {
            CreateFolderIfMissing("Assets", "Art");
            CreateFolderIfMissing(ArtRoot, "Meshes");
            CreateFolderIfMissing(ArtRoot, "Prefabs");
            CreateFolderIfMissing(ArtRoot, "Textures");
            CreateFolderIfMissing("Assets/Art/Textures", "Masks");
            CreateFolderIfMissing(ArtRoot, "Materials");
            CreateFolderIfMissing("Assets", "ScriptableObjects");
            CreateFolderIfMissing("Assets", "Scenes");
        }

        private static void CreateFolderIfMissing(string parent, string child)
        {
            string fullPath = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Material CreateOrGetMasterMaterial()
        {
            string matPath = $"{MaterialsPath}/M_Pony_Master.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader toonShader = Shader.Find("MangoPony/ToonMaster");
                if (toonShader == null)
                {
                    toonShader = Shader.Find("Universal Render Pipeline/Lit");
                }

                mat = new Material(toonShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            return mat;
        }

        private static void GenerateMaskTextures()
        {
            // Face Marks
            foreach (FaceMarkType type in Enum.GetValues(typeof(FaceMarkType)))
            {
                if (type == FaceMarkType.None) continue;
                Texture2D tex = PonyProceduralTextureGenerator.CreateFaceMarkTexture(type);
                SaveTextureAsPNG(tex, $"{TexturesPath}/FaceMark_{type}.png");
            }

            // Leg Marks
            foreach (LegMarkType type in Enum.GetValues(typeof(LegMarkType)))
            {
                if (type == LegMarkType.None) continue;
                Texture2D tex = PonyProceduralTextureGenerator.CreateLegMarkTexture(type);
                SaveTextureAsPNG(tex, $"{TexturesPath}/LegMark_{type}.png");
            }

            // Body Patterns
            foreach (BodyPatternType type in Enum.GetValues(typeof(BodyPatternType)))
            {
                if (type == BodyPatternType.None) continue;
                Texture2D tex = PonyProceduralTextureGenerator.CreateBodyPatternTexture(type);
                SaveTextureAsPNG(tex, $"{TexturesPath}/Pattern_{type}.png");
            }

            // Coat Nuances
            foreach (CoatNuanceType type in Enum.GetValues(typeof(CoatNuanceType)))
            {
                if (type == CoatNuanceType.None) continue;
                Texture2D tex = PonyProceduralTextureGenerator.CreateCoatNuanceTexture(type);
                SaveTextureAsPNG(tex, $"{TexturesPath}/Nuance_{type}.png");
            }

            AssetDatabase.Refresh();
        }

        private static void SaveTextureAsPNG(Texture2D tex, string relativePath)
        {
            byte[] bytes = tex.EncodeToPNG();
            string fullDiskPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            File.WriteAllBytes(fullDiskPath, bytes);
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
        }

        private static void GenerateProxyMeshesAndPrefabs(Material mat)
        {
            // 1. Corps (Body)
            foreach (BodyType type in Enum.GetValues(typeof(BodyType)))
            {
                Mesh mesh = PonyPlaceholderMeshGenerator.CreateBodyMesh(type);
                SaveMeshAndPrefab(mesh, $"{MeshesPath}/Mesh_Body_{type}.asset", $"{PrefabsPath}/Prefab_Body_{type}.prefab", mat);
            }

            // 2. Têtes (Head)
            foreach (HeadType type in Enum.GetValues(typeof(HeadType)))
            {
                Mesh mesh = PonyPlaceholderMeshGenerator.CreateHeadMesh(type);
                SaveMeshAndPrefab(mesh, $"{MeshesPath}/Mesh_Head_{type}.asset", $"{PrefabsPath}/Prefab_Head_{type}.prefab", mat, new Vector3(0f, 1.25f, 0.65f));
            }

            // 3. Sabots (Hooves)
            foreach (HoofType type in Enum.GetValues(typeof(HoofType)))
            {
                Mesh mesh = PonyPlaceholderMeshGenerator.CreateHoofMesh(type);
                // Crée un prefab combinant les 4 sabots aux 4 pieds
                GameObject hoovesRoot = new GameObject($"Prefab_Hoof_{type}");
                Vector3[] hoofPositions = new Vector3[]
                {
                    new Vector3(-0.25f, 0.1f,  0.4f), // FL
                    new Vector3( 0.25f, 0.1f,  0.4f), // FR
                    new Vector3(-0.25f, 0.1f, -0.4f), // BL
                    new Vector3( 0.25f, 0.1f, -0.4f)  // BR
                };

                SaveMeshAsset(mesh, $"{MeshesPath}/Mesh_Hoof_{type}.asset");

                for (int i = 0; i < hoofPositions.Length; i++)
                {
                    GameObject child = new GameObject($"Hoof_{i}");
                    child.transform.SetParent(hoovesRoot.transform, false);
                    child.transform.localPosition = hoofPositions[i];
                    var mf = child.AddComponent<MeshFilter>();
                    var mr = child.AddComponent<MeshRenderer>();
                    mf.sharedMesh = mesh;
                    mr.sharedMaterial = mat;
                }

                PrefabUtility.SaveAsPrefabAsset(hoovesRoot, $"{PrefabsPath}/Prefab_Hoof_{type}.prefab");
                UnityEngine.Object.DestroyImmediate(hoovesRoot);
            }

            // 4. Crinières (Mane)
            foreach (ManeType type in Enum.GetValues(typeof(ManeType)))
            {
                if (type == ManeType.None) continue;
                Mesh mesh = PonyPlaceholderMeshGenerator.CreateManeMesh(type);
                SaveMeshAndPrefab(mesh, $"{MeshesPath}/Mesh_Mane_{type}.asset", $"{PrefabsPath}/Prefab_Mane_{type}.prefab", mat, new Vector3(0f, 1.15f, 0.35f));
            }

            // 5. Queues (Tail)
            foreach (TailType type in Enum.GetValues(typeof(TailType)))
            {
                if (type == TailType.None) continue;
                Mesh mesh = PonyPlaceholderMeshGenerator.CreateTailMesh(type);
                SaveMeshAndPrefab(mesh, $"{MeshesPath}/Mesh_Tail_{type}.asset", $"{PrefabsPath}/Prefab_Tail_{type}.prefab", mat, new Vector3(0f, 0.85f, -0.55f));
            }

            // 6. Cornes (Horns)
            foreach (HornType type in Enum.GetValues(typeof(HornType)))
            {
                if (type == HornType.None) continue;
                Mesh mesh = PonyPlaceholderMeshGenerator.CreateHornMesh(type);
                SaveMeshAndPrefab(mesh, $"{MeshesPath}/Mesh_Horn_{type}.asset", $"{PrefabsPath}/Prefab_Horn_{type}.prefab", mat, new Vector3(0f, 1.45f, 0.75f));
            }

            // 7. Ailes (Wings)
            foreach (WingType type in Enum.GetValues(typeof(WingType)))
            {
                if (type == WingType.None) continue;
                Mesh mesh = PonyPlaceholderMeshGenerator.CreateWingMesh(type);
                SaveMeshAsset(mesh, $"{MeshesPath}/Mesh_Wing_{type}.asset");

                GameObject wingsRoot = new GameObject($"Prefab_Wing_{type}");
                // Aile gauche
                GameObject wingL = new GameObject("Wing_L");
                wingL.transform.SetParent(wingsRoot.transform, false);
                wingL.transform.localPosition = new Vector3(-0.35f, 1.05f, 0.15f);
                wingL.transform.localRotation = Quaternion.Euler(0f, -25f, 15f);
                var mfL = wingL.AddComponent<MeshFilter>();
                var mrL = wingL.AddComponent<MeshRenderer>();
                mfL.sharedMesh = mesh;
                mrL.sharedMaterial = mat;

                // Aile droite (inversée sur X)
                GameObject wingR = new GameObject("Wing_R");
                wingR.transform.SetParent(wingsRoot.transform, false);
                wingR.transform.localPosition = new Vector3(0.35f, 1.05f, 0.15f);
                wingR.transform.localRotation = Quaternion.Euler(0f, 25f, -15f);
                wingR.transform.localScale = new Vector3(-1f, 1f, 1f);
                var mfR = wingR.AddComponent<MeshFilter>();
                var mrR = wingR.AddComponent<MeshRenderer>();
                mfR.sharedMesh = mesh;
                mrR.sharedMaterial = mat;

                PrefabUtility.SaveAsPrefabAsset(wingsRoot, $"{PrefabsPath}/Prefab_Wing_{type}.prefab");
                UnityEngine.Object.DestroyImmediate(wingsRoot);
            }
        }

        private static void SaveMeshAsset(Mesh mesh, string path)
        {
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                existing.Clear();
                EditorUtility.CopySerialized(mesh, existing);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, path);
            }
        }

        private static void SaveMeshAndPrefab(Mesh mesh, string meshPath, string prefabPath, Material mat, Vector3 localPos = default)
        {
            SaveMeshAsset(mesh, meshPath);

            GameObject tempObj = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            tempObj.transform.localPosition = localPos;
            var mf = tempObj.AddComponent<MeshFilter>();
            var mr = tempObj.AddComponent<MeshRenderer>();
            mf.sharedMesh = mesh;
            mr.sharedMaterial = mat;

            PrefabUtility.SaveAsPrefabAsset(tempObj, prefabPath);
            UnityEngine.Object.DestroyImmediate(tempObj);
        }

        private static PonyCustomizationDatabase CreateOrUpdateDatabase()
        {
            PonyCustomizationDatabase db = AssetDatabase.LoadAssetAtPath<PonyCustomizationDatabase>(DatabasePath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<PonyCustomizationDatabase>();
                AssetDatabase.CreateAsset(db, DatabasePath);
            }

            // Enregistrement des meshes & prefabs
            foreach (BodyType t in Enum.GetValues(typeof(BodyType)))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Prefab_Body_{t}.prefab");
                db.RegisterBody(t, t.ToString(), prefab);
            }

            foreach (HeadType t in Enum.GetValues(typeof(HeadType)))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Prefab_Head_{t}.prefab");
                db.RegisterHead(t, t.ToString(), prefab);
            }

            foreach (HoofType t in Enum.GetValues(typeof(HoofType)))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Prefab_Hoof_{t}.prefab");
                db.RegisterHoof(t, t.ToString(), prefab);
            }

            foreach (ManeType t in Enum.GetValues(typeof(ManeType)))
            {
                GameObject prefab = t != ManeType.None ? AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Prefab_Mane_{t}.prefab") : null;
                db.RegisterMane(t, t.ToString(), prefab);
            }

            foreach (TailType t in Enum.GetValues(typeof(TailType)))
            {
                GameObject prefab = t != TailType.None ? AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Prefab_Tail_{t}.prefab") : null;
                db.RegisterTail(t, t.ToString(), prefab);
            }

            foreach (HornType t in Enum.GetValues(typeof(HornType)))
            {
                GameObject prefab = t != HornType.None ? AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Prefab_Horn_{t}.prefab") : null;
                db.RegisterHorn(t, t.ToString(), prefab);
            }

            foreach (WingType t in Enum.GetValues(typeof(WingType)))
            {
                GameObject prefab = t != WingType.None ? AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Prefab_Wing_{t}.prefab") : null;
                db.RegisterWing(t, t.ToString(), prefab);
            }

            // Enregistrement des textures de masques
            foreach (FaceMarkType t in Enum.GetValues(typeof(FaceMarkType)))
            {
                Texture2D tex = t != FaceMarkType.None ? AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesPath}/FaceMark_{t}.png") : null;
                db.RegisterFaceMark(t, t.ToString(), tex);
            }

            foreach (LegMarkType t in Enum.GetValues(typeof(LegMarkType)))
            {
                Texture2D tex = t != LegMarkType.None ? AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesPath}/LegMark_{t}.png") : null;
                db.RegisterLegMark(t, t.ToString(), tex);
            }

            foreach (BodyPatternType t in Enum.GetValues(typeof(BodyPatternType)))
            {
                Texture2D tex = t != BodyPatternType.None ? AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesPath}/Pattern_{t}.png") : null;
                db.RegisterBodyPattern(t, t.ToString(), tex);
            }

            foreach (CoatNuanceType t in Enum.GetValues(typeof(CoatNuanceType)))
            {
                Texture2D tex = t != CoatNuanceType.None ? AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesPath}/Nuance_{t}.png") : null;
                db.RegisterCoatNuance(t, t.ToString(), tex);
            }

            EditorUtility.SetDirty(db);
            return db;
        }

        private static void CreateCustomizerScene(PonyCustomizationDatabase db)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 1. Sol / Podium
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "Podium";
            floor.transform.position = new Vector3(0f, -0.05f, 0f);
            floor.transform.localScale = new Vector3(4f, 0.05f, 4f);

            // 2. Éclairage
            var lightObj = GameObject.Find("Directional Light");
            if (lightObj != null)
            {
                lightObj.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
                var light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    light.intensity = 1.2f;
                    light.color = new Color(1f, 0.98f, 0.95f);
                }
            }

            // 3. Poney Rig
            GameObject ponyObj = PonyAvatarBuilder.CreateBasePonyRig("MangoPony_CustomizerTarget");
            var modController = ponyObj.AddComponent<PonyModularAttachmentController>();
            var matController = ponyObj.AddComponent<PonyMaterialController>();
            var customizerMgr = ponyObj.AddComponent<PonyCustomizerManager>();

            // Injection Database via SerializedObject
            var serializedMgr = new SerializedObject(customizerMgr);
            serializedMgr.FindProperty("database").objectReferenceValue = db;
            serializedMgr.ApplyModifiedProperties();

            // 4. Caméra Orbitale
            var camObj = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera", typeof(Camera));
            var orbitCam = camObj.GetComponent<PonyOrbitCamera>();
            if (orbitCam == null)
            {
                orbitCam = camObj.AddComponent<PonyOrbitCamera>();
            }
            orbitCam.Target = ponyObj.transform;

            // 5. Interface UI (Canvas)
            PonyCustomizerSceneUIBuilder.BuildCustomizerCanvas(ponyObj, orbitCam);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }
    }
}
#endif

