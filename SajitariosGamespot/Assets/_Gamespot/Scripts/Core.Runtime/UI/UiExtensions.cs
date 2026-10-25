using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>Ayudas de tamaño y jerarquía para la UI construida por código.</summary>
    public static class UiExtensions
    {
        public static T GetOrAdd<T>(this GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        /// <summary>
        /// Fija el tamaño en el layout: valores negativos = sin cambiar. <paramref name="flexWidth"/> y
        /// <paramref name="flexHeight"/> reparten el espacio sobrante (como <c>Expanded</c>).
        /// </summary>
        public static T Sized<T>(this T component, float width = -1f, float height = -1f, float flexWidth = -1f, float flexHeight = -1f) where T : Component
        {
            var element = component.gameObject.GetOrAdd<LayoutElement>();
            if (width >= 0f)
            {
                element.minWidth = width;
                element.preferredWidth = width;
            }
            if (height >= 0f)
            {
                element.minHeight = height;
                element.preferredHeight = height;
            }
            if (flexWidth >= 0f) element.flexibleWidth = flexWidth;
            if (flexHeight >= 0f) element.flexibleHeight = flexHeight;
            return component;
        }

        /// <summary>Altura mínima (objetivos táctiles de 48 o más).</summary>
        public static T MinHeight<T>(this T component, float height) where T : Component
        {
            component.gameObject.GetOrAdd<LayoutElement>().minHeight = height;
            return component;
        }

        /// <summary>Destruye todos los hijos (para reconstruir una lista).</summary>
        public static void ClearChildren(this Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                child.SetActive(false);
                Object.Destroy(child);
            }
        }

        public static RectTransform Rect(this Component component) => (RectTransform)component.transform;
    }
}
