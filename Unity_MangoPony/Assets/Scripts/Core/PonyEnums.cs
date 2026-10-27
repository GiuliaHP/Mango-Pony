using System;

namespace MangoPony.Core
{
    /// <summary>
    /// Morphologie générale du corps du poney.
    /// </summary>
    public enum BodyType
    {
        Classique = 0,
        Trait = 1,
        Arabe = 2
    }

    /// <summary>
    /// Morphologie de la tête du poney (profil osseux).
    /// </summary>
    public enum HeadType
    {
        Plate = 0,
        Bombee = 1,
        Creuse = 2
    }

    /// <summary>
    /// Types de sabots et bas de membres.
    /// </summary>
    public enum HoofType
    {
        Classiques = 0,
        Fendus = 1,
        Fanons = 2
    }

    /// <summary>
    /// Styles de crinière (Mane).
    /// </summary>
    public enum ManeType
    {
        None = 0,
        Classique = 1,
        Brosse = 2,
        Tresse = 3,
        Wavy = 4,
        Sauvage = 5
    }

    /// <summary>
    /// Styles de queue (Tail).
    /// </summary>
    public enum TailType
    {
        None = 0,
        Classique = 1,
        Licorne = 2,
        Tresses = 3,
        Sirene = 4,
        Wavy = 5
    }

    /// <summary>
    /// Accessoires fantastiques : Cornes.
    /// </summary>
    public enum HornType
    {
        None = 0,
        Droite = 1,
        Courbee = 2,
        Rhino = 3,
        Chevre = 4,
        Cerf = 5,
        Bouc = 6
    }

    /// <summary>
    /// Accessoires fantastiques : Ailes.
    /// </summary>
    public enum WingType
    {
        None = 0,
        Plumes = 1,
        Dragon = 2,
        Mini = 3
    }

    /// <summary>
    /// Marques en tête (balzanes de tête / marques blanches faciales).
    /// </summary>
    public enum FaceMarkType
    {
        None = 0,
        Etoile = 1,
        Pelote = 2,
        ListeFine = 3,
        ListeLarge = 4,
        EtoileListee = 5
    }

    /// <summary>
    /// Marques de membres (balzanes aux jambes).
    /// </summary>
    public enum LegMarkType
    {
        None = 0,
        Couronne = 1,
        PetiteBalzane = 2,
        MoyenneBalzane = 3,
        GrandeBalzane = 4,
        Rayures = 5
    }

    /// <summary>
    /// Robes et motifs corporels (Patterns).
    /// </summary>
    public enum BodyPatternType
    {
        None = 0,
        Appaloosa = 1,
        MiniTobiano = 2,
        Cape = 3,
        Tobiano = 4
    }

    /// <summary>
    /// Nuances et dégradés de robe (Nuances).
    /// </summary>
    public enum CoatNuanceType
    {
        None = 0,
        Pommele = 1,
        Pangare = 2,
        Isabelle = 3
    }

    /// <summary>
    /// Identification des 4 membres pour la personnalisation individuelle des balzanes.
    /// </summary>
    public enum LegSlot
    {
        FrontLeft = 0,
        FrontRight = 1,
        BackLeft = 2,
        BackRight = 3
    }

    /// <summary>
    /// Points de focus caméra prédéfinis pour la vue 3D.
    /// </summary>
    public enum CameraFocusTarget
    {
        WholeBody = 0,
        Head = 1,
        Hooves = 2,
        Wings = 3
    }
}

