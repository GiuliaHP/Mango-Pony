# 🐴 Pony Data Linker

[English](#english) | [Français](#français)

---

<a name="english"></a>
## 🇬🇧 English

### Overview
**`PonyDataLinker`** is an automated Unity Editor tool that finds all 3D pony prefabs in `Assets/Prefabs/Pony/` and links them to the corresponding `CustomizationCategoryData` ScriptableObjects in `Assets/Scriptables/` by matching their names.

### How to Use

You can run the tool in two easy ways:

#### Option 1: Top Menu
In Unity's top menu bar, click:
> **`Mango Pony` $\rightarrow$ `Link Prefabs to Data Assets`**

#### Option 2: Inspector Button
1. Select the GameObject with the **`CustomizationUIGenerator`** component in your scene.
2. In the Inspector, click the blue button:
> **`[ Lier les Prefabs aux ScriptableObjects (par nom) ]`**
*(Located right above the "Générer l'Interface UI" button)*

### How Name Matching Works
1. **Slot Categorization**: Prefabs must match the corresponding element slot:
   * `Prefab_Body_*` $\rightarrow$ Body slots
   * `Prefab_Head_*` $\rightarrow$ Head slots
   * `Prefab_Hooves_*` $\rightarrow$ Hooves slots
   * `Prefab_Horn_*` $\rightarrow$ Horn slots
   * `Prefab_Mane_*` $\rightarrow$ Mane slots
   * `Prefab_Tail_*` $\rightarrow$ Tail slots
   * `Prefab_Wing_*` $\rightarrow$ Wing slots
2. **Fuzzy Name Matching**: Accents (`é`/`è` $\leftrightarrow$ `e`), casing, and special characters are ignored (e.g. `Bombée` matches `Prefab_Head_Bombee`, `Chèvre` matches `Prefab_Horn_Chevre`).
3. **Plurals & Singulars**: Variations like `Tresses` $\leftrightarrow$ `Prefab_Tail_Tresses` / `tresse` are handled automatically.
4. **Empty Options**: Shapes named `Aucune`, `Aucun`, `None`, or left blank are automatically set to `null`.

### Typical Workflow
1. Create or import your new 3D mesh prefab into `Assets/Prefabs/Pony/` following the naming convention: `Prefab_<Slot>_<ShapeName>.prefab`.
2. Add the corresponding shape name in your `CustomizationCategoryData` asset (e.g. `Datas_Hair.asset`).
3. Click **`Lier les Prefabs aux ScriptableObjects (par nom)`** (or use the top menu).
4. Click **`Générer l'Interface UI (Hors Play Mode)`** to rebuild the UI with the new linked prefabs.

---

<a name="français"></a>
## 🇫🇷 Français

### Vue d'ensemble
**`PonyDataLinker`** est un outil Éditeur pour Unity qui détecte tous les prefabs 3D situés dans `Assets/Prefabs/Pony/` et les assigne automatiquement aux ScriptableObjects `CustomizationCategoryData` dans `Assets/Scriptables/` en faisant correspondre leurs noms.

### Comment l'utiliser

Vous pouvez lancer l'outil de deux façons très simples :

#### Option 1 : Menu supérieur Unity
Dans la barre de menus en haut de Unity, cliquez sur :
> **`Mango Pony` $\rightarrow$ `Link Prefabs to Data Assets`**

#### Option 2 : Bouton dans l'Inspecteur
1. Sélectionnez le GameObject porteur du composant **`CustomizationUIGenerator`** dans votre scène.
2. Dans l'Inspecteur, cliquez sur le bouton bleu :
> **`[ Lier les Prefabs aux ScriptableObjects (par nom) ]`**
*(Situé juste au-dessus du bouton "Générer l'Interface UI")*

### Fonctionnement de la correspondance des noms
1. **Validation par emplacement (Slot)** : Le prefab doit correspondre au slot de l'élément :
   * `Prefab_Body_*` $\rightarrow$ Emplacement Corps
   * `Prefab_Head_*` $\rightarrow$ Emplacement Tête
   * `Prefab_Hooves_*` $\rightarrow$ Emplacement Sabots
   * `Prefab_Horn_*` $\rightarrow$ Emplacement Corne
   * `Prefab_Mane_*` $\rightarrow$ Emplacement Crinière
   * `Prefab_Tail_*` $\rightarrow$ Emplacement Queue
   * `Prefab_Wing_*` $\rightarrow$ Emplacement Ailes
2. **Tolérance aux accents et à la casse** : Les accents (`é`/`è` $\leftrightarrow$ `e`), la casse et les caractères spéciaux sont automatiquement normalisés (ex : `Bombée` $\leftrightarrow$ `Prefab_Head_Bombee`, `Chèvre` $\leftrightarrow$ `Prefab_Horn_Chevre`).
3. **Singulier / Pluriel** : Les variantes comme `Tresses` $\leftrightarrow$ `Prefab_Tail_Tresses` / `tresse` sont prises en compte.
4. **Options vides** : Les formes nommées `Aucune`, `Aucun`, `None`, ou laissées vides ont leur référence réinitialisée à `null`.

### Cycle de travail recommandé
1. Créez ou importez votre nouveau prefab 3D dans `Assets/Prefabs/Pony/` en respectant la convention : `Prefab_<Slot>_<NomDeForme>.prefab`.
2. Ajoutez le nom de la variation dans le ScriptableObject concerné (ex : `Datas_Hair.asset`).
3. Cliquez sur **`Lier les Prefabs aux ScriptableObjects (par nom)`** (ou via le menu `Mango Pony`).
4. Cliquez sur **`Générer l'Interface UI (Hors Play Mode)`** pour mettre à jour l'interface avec les nouveaux boutons et prefabs liés.
