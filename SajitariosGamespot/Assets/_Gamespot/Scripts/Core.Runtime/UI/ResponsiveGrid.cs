using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Rejilla que reparte el ancho disponible en columnas iguales (2, o 3 si el ancho supera
    /// <see cref="WideBreakpoint"/>) con una proporción ancho/alto fija por celda, como el <c>GridView</c> del menú.
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class ResponsiveGrid : MonoBehaviour
    {
        public const float WideBreakpoint = 600f;

        private GridLayoutGroup _grid;
        private float _aspect = 0.9f;
        private float _lastWidth = -1f;

        public static ResponsiveGrid Create(Transform parent, float spacing, float aspect)
        {
            var go = new GameObject("Grid", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var grid = go.AddComponent<GridLayoutGroup>();
            grid.spacing = new Vector2(spacing, spacing);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            var responsive = go.AddComponent<ResponsiveGrid>();
            responsive._grid = grid;
            responsive._aspect = aspect;
            return responsive;
        }

        public int Columns => _grid.constraintCount;

        private void OnRectTransformDimensionsChange() => Refresh();

        private void LateUpdate() => Refresh();

        private void Refresh()
        {
            if (_grid == null) return;
            var width = ((RectTransform)transform).rect.width;
            if (width <= 0f || Mathf.Approximately(width, _lastWidth)) return;
            _lastWidth = width;
            var columns = width > WideBreakpoint ? 3 : 2;
            _grid.constraintCount = columns;
            var cell = (width - _grid.spacing.x * (columns - 1)) / columns;
            _grid.cellSize = new Vector2(cell, cell / _aspect);
        }
    }
}
