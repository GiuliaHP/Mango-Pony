using System;
using UnityEngine;

namespace MangoPony.Save
{
    /// <summary>
    /// Structure sérialisable pour stocker des couleurs RGBA avec JsonUtility.
    /// Fournit des conversions implicites vers et depuis UnityEngine.Color.
    /// </summary>
    [Serializable]
    public struct SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializableColor(float r, float g, float b, float a = 1.0f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public SerializableColor(Color color)
        {
            this.r = color.r;
            this.g = color.g;
            this.b = color.b;
            this.a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public static implicit operator Color(SerializableColor sc) => sc.ToColor();
        public static implicit operator SerializableColor(Color c) => new SerializableColor(c);

        public string ToHexString()
        {
            return ColorUtility.ToHtmlStringRGBA(ToColor());
        }

        public static SerializableColor FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return new SerializableColor(color);
            }
            return new SerializableColor(1, 1, 1, 1);
        }
    }
}

