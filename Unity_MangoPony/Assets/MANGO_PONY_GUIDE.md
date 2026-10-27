# Mango Pony - 3D Customizer Prototype (Unity 6 / URP)

Bienvenue dans l'architecture du prototype de personnalisation 3D pour **Mango Pony**, développé pour Unity 6 (`6000.4.0f1`) et Universal Render Pipeline (URP).

---

## 🚀 Démarrage Rapide en 1 Clic

1. Dans la barre de menus de Unity, cliquez sur :
   **`Mango Pony > 1. Setup Complet (Assets, Database & Scène)`**
2. Cette commande exécute automatiquement :
   - La génération des 17 masques de textures procédurales (Face marks, Balzanes, Motifs, Nuances) dans `Assets/Art/Textures/Masks/`.
   - La génération des meshes 3D proxy et prefabs modulaires dans `Assets/Art/Meshes/` et `Assets/Art/Prefabs/`.
   - La création et configuration du ScriptableObject `PonyCustomizationDatabase` dans `Assets/ScriptableObjects/`.
   - La génération de la scène interactive complète `Assets/Scenes/MangoPonyCustomizer.unity` comprenant l'armature, la caméra orbitale, l'éclairage Toon et l'interface Canvas.
3. Ouvrez la scène `Assets/Scenes/MangoPonyCustomizer.unity` et appuyez sur **Play** !

---

## 🏗️ Architecture & Modules

### 1. Système Modulaire de Meshes (`Assets/Scripts/Customization/`)
- **`PonyEnums.cs`** : Définition de l'ensemble des types énumérés (Corps, Tête, Sabots, Crinières, Queues, Cornes, Ailes, Balzanes, Robes, etc.).
- **`PonyAttachmentSockets.cs`** : Définition des points d'ancrage fixes et du squelette partagé (`RootBone`, `HeadSocket`, `HornSocket`, `ManeSocket`, `TailSocket`, `LeftWingSocket`, `RightWingSocket`, `HoofSockets`).
- **`SkinnedMeshBoneRemapper.cs`** : Remappe dynamiquement les os d'un `SkinnedMeshRenderer` instancié vers le squelette partagé du poney par correspondance de noms d'os.
- **`PonyModularAttachmentController.cs`** : Gère l'instanciation, le remplacement et la destruction des instances pour chaque slot (`Body`, `Head`, `Hooves`, `Mane`, `Tail`, `Horns`, `Wings`).
- **`PonyPlaceholderMeshGenerator.cs` & `PonyAvatarBuilder.cs`** : Génération procédurale du squelette et des meshes proxy pour tester immédiatement l'assemblage.

### 2. Shader Toon & Matériaux en Temps Réel (`Assets/Shaders/` & `Assets/Scripts/Shaders/`)
- **`MangoPonyToon.shader`** : Shader URP HLSL Cel-Shading moderne supportant :
  - Éclairage Toon / demi-Lambert avec seuil et douceur d'ombrage configurables.
  - Teinte d'ombrage personnalisée (`_ShadeColor`).
  - Rim lighting stylisé (Fresnel).
  - Passes URP : `UniversalForward`, `ShadowCaster`, `DepthOnly`.
  - **Empilement multicouche de masques de textures** :
    1. Couche de base (`_BaseColor` + `_BaseMap`).
    2. Nuances de robe (`_CoatNuanceTex`, `_CoatNuanceColor`).
    3. Motifs corporels (`_BodyPatternTex`, `_BodyPatternColor`).
    4. Balzanes aux 4 membres avec mode synchronisé ou 4 textures indépendantes (`_UseIndividualLegs`).
    5. Marques en tête (`_FaceMarkTex`, `_FaceMarkColor`).
- **`PonyMaterialController.cs`** : Contrôleur gérant les instances de matériaux dynamiques pour chaque partie (Corps, Crins, Yeux, Sabots, Cornes, Ailes) et appliquant les propriétés via IDs de shaders optimisés.

### 3. Système d'Interface UI (`Assets/Scripts/UI/`)
- **`PonyTabController.cs`** : Navigation par onglets (Corps / Tête & Crins / Marques & Couleurs / Accessoires) avec cadrage automatique de la caméra 3D selon la catégorie sélectionnée.
- **`UIOptionSelector.cs`** : Sélecteur carrousel réutilisable avec flèches précédent/suivant et libellé d'option.
- **`UIFlexibleColorPicker.cs`** : Sélecteur de couleur interactif avec curseurs RGB, prévisualisation swatch et palette de raccourcis.
- **`PonyCustomizerUI.cs`** : Contrôleur UI principal reliant les événements de l'UI au `PonyCustomizerManager`.

### 4. Caméra & Inspection 3D (`Assets/Scripts/Camera/`)
- **`PonyOrbitCamera.cs`** : Caméra orbitale 360° fluide compatible avec le **New Input System** de Unity 6 (`UnityEngine.InputSystem`), avec gestion du zoom molette, détection de survol UI, points de focus prédéfinis (`WholeBody`, `Head`, `Hooves`, `Wings`) et mode turntable (auto-rotation).

### 5. Persistance & Sauvegarde (`Assets/Scripts/Save/`)
- **`PonyData.cs`** : Modèle de données complet sérialisable contenant les indices de meshes, masques de textures, booléen de synchronisation des membres, et les couleurs RGBA (`SerializableColor`).
- **`PonySaveSystem.cs`** : Sauvegarde et chargement JSON dans `Application.persistentDataPath` avec gestion des erreurs et support multi-profils.

### 6. Outils Éditeur (`Assets/Scripts/Editor/`)
- **`PonyCustomizerSetupWizard.cs`** : Wizard complet en 1 clic.
- **`PonyCustomizerSceneUIBuilder.cs`** : Construction procédurale du Canvas et de l'arborescence UI.
- **`PonyCustomizerManagerEditor.cs`** : Inspecteur Unity avec boutons d'actions en mode édition.

