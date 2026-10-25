using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>Ayudas para colocar RectTransforms por código.</summary>
    public static class UiLayout
    {
        /// <summary>Estira el rect a todo su padre, sin márgenes.</summary>
        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Crea un hijo vacío con RectTransform estirado al padre.</summary>
        public static RectTransform CreateStretched(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            Stretch(rect);
            return rect;
        }
    }
}
