# 🐴 Pony Data Linker

[English](#english) | [Français](#français)

---

<a name="english"></a>
## 🇬🇧 English

### Overview
**`PonyDataLinker`** is an automated Unity Editor tool that finds all 3D pony prefabs in `Assets/Prefabs/Pony/`, preview icons in `Assets/Sprites/PreviewIcons.psd`, and shader mask textures in `Assets/Textures/Masks/`, and automatically links them to the corresponding `CustomizationCategoryData` ScriptableObjects in `Assets/Scriptables/` by name.

### How to Use

You can run the tool in two easy ways:

#### Option 1: Top Menu
In Unity's top menu bar, click:
> **`Mango Pony` $\rightarrow$ `Link Prefabs, Icons & Textures to Data Assets`**

#### Option 2: Inspector Button
1. Select the GameObject with the **`CustomizationUIGenerator`** component in your scene.
2. In the Inspector, click the blue button:
> **`[ Lier les Prefabs aux ScriptableObjects (par nom) ]`**
*(Located right above the "Générer l'Interface UI" button)*

### How Name Matching Works
1. **3D Mesh Prefabs (`meshPrefab`)**:
   * Matches `Prefab_<Slot>_<ShapeName>.prefab` (e.g. `Straight` $\rightarrow$ `Prefab_Horn_Droite`, `Feathered` $\rightarrow$ `Prefab_Wing_Plumes`).
   * Supports bilingual EN $\leftrightarrow$ FR synonyms (e.g., `Straight`/`Droite`, `Brush`/`Brosse`, `Roman`/`Bombée`, etc.).
2. **Preview Icons (`previewIcon`)**:
   * Automatically looks up sprites inside `PreviewIcons.psd` matching the element and variation name (e.g., `Horn_Straight`, `Wings_Feathers`, `Mane_Classic`, `BodyPattern_Appaloosa`, `HeadPattern_Star`, `LegsPaterns_Crown`).
3. **Mask Textures (`patternTexture`)**:
   * Automatically assigns mask textures from `Assets/Textures/Masks/` (e.g., `Pattern_Appaloosa.png`, `Nuance_Pommele.png`, `LegMark_Couronne.png`, `FaceMark_Etoile.png`).
4. **Empty Options**: Shapes or patterns named `None`, `Aucune`, `Aucun`, or `Solid` automatically set their mesh or texture reference to `null`.
5. **Non-referenced items**: Any items without matching prefabs, sprites, or textures (such as Deer Horns, Stripes, or Fleabitten) are excluded so that only valid assets appear in your customization UI.

### Typical Workflow
1. Create or import your new 3D mesh prefab into `Assets/Prefabs/Pony/` and/or add sprite icons in `PreviewIcons.psd`.
2. Add the corresponding variation in your `CustomizationCategoryData` asset (e.g. `Datas_Accessories.asset`).
3. Click **`Lier les Prefabs aux ScriptableObjects (par nom)`** (or use the top menu).
4. Click **`Générer l'Interface UI (Hors Play Mode)`** to rebuild the UI: the generator automatically targets the child Image named `"Preview"` inside each instantiated preview button (`Button_Preview`, `Button_Preview_Pattern`) and replaces its sprite with the variant's associated `previewIcon`.

---

<a name="français"></a>
## 🇫🇷 Français

### Vue d'ensemble
**`PonyDataLinker`** est un outil Éditeur pour Unity qui détecte tous les prefabs 3D situés dans `Assets/Prefabs/Pony/`, les icônes de prévisualisation dans `Assets/Sprites/PreviewIcons.psd` et les textures de masques dans `Assets/Textures/Masks/`, et les assigne automatiquement aux ScriptableObjects `CustomizationCategoryData` dans `Assets/Scriptables/` selon leurs noms.

### Comment l'utiliser

Vous pouvez lancer l'outil de deux façons très simples :

#### Option 1 : Menu supérieur Unity
Dans la barre de menus en haut de Unity, cliquez sur :
> **`Mango Pony` $\rightarrow$ `Link Prefabs, Icons & Textures to Data Assets`**

#### Option 2 : Bouton dans l'Inspecteur
1. Sélectionnez le GameObject porteur du composant **`CustomizationUIGenerator`** dans votre scène.
2. Dans l'Inspecteur, cliquez sur le bouton bleu :
> **`[ Lier les Prefabs aux ScriptableObjects (par nom) ]`**
*(Situé juste au-dessus du bouton "Générer l'Interface UI")*

### Fonctionnement de la correspondance des noms
1. **Prefabs 3D (`meshPrefab`)** :
   * Associe les prefabs `Prefab_<Slot>_<NomDeForme>.prefab` (ex : `Straight` $\rightarrow$ `Prefab_Horn_Droite`, `Feathered` $\rightarrow$ `Prefab_Wing_Plumes`).
   * Gère les synonymes bilingues EN $\leftrightarrow$ FR (`Straight`/`Droite`, `Brush`/`Brosse`, `Roman`/`Bombée`, etc.).
2. **Icônes de prévisualisation (`previewIcon`)** :
   * Trouve automatiquement les sprites découpés dans `PreviewIcons.psd` (ex : `Horn_Straight`, `Wings_Feathers`, `Mane_Classic`, `BodyPattern_Appaloosa`, `HeadPattern_Star`, `LegsPaterns_Crown`).
3. **Textures de masques (`patternTexture`)** :
   * Assigne automatiquement les masques dans `Assets/Textures/Masks/` (ex : `Pattern_Appaloosa.png`, `Nuance_Pommele.png`, `LegMark_Couronne.png`, `FaceMark_Etoile.png`).
4. **Options vides** : Les formes ou motifs nommés `None`, `Aucune`, `Aucun`, ou `Solid` voient leur mesh ou texture réinitialisé à `null`.
5. **Éléments non référencés** : Tout élément ne possédant ni prefab ni sprite ou texture (comme les cornes de cerf "Deer", les rayures de pattes "Stripes", ou le moucheté "Fleabitten") est retiré pour que seuls les éléments complets apparaissent dans l'UI.

### Cycle de travail recommandé
1. Créez ou importez votre nouveau prefab 3D dans `Assets/Prefabs/Pony/` et/ou vos icônes dans `PreviewIcons.psd`.
2. Ajoutez la variation dans le ScriptableObject concerné (ex : `Datas_Accessories.asset`).
3. Cliquez sur **`Lier les Prefabs aux ScriptableObjects (par nom)`** (ou via le menu `Mango Pony`).
4. Cliquez sur **`Générer l'Interface UI (Hors Play Mode)`** pour mettre à jour l'interface : le générateur cible automatiquement l'enfant Image nommé `"Preview"` au sein de chaque bouton de prévisualisation (`Button_Preview`, `Button_Preview_Pattern`) et remplace son sprite par le `previewIcon` associé.
