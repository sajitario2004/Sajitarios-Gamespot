using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>Rejilla de chips que reparte el ancho en N columnas (temáticas de la Trivia, categorías de palabras).</summary>
    public sealed class ResponsiveChipGrid : MonoBehaviour
    {
        public int Columns = 2;
        private GridLayoutGroup _grid;
        private float _lastWidth = -1f;

        /// <summary>Rejilla vacía de <paramref name="columns"/> columnas con chips de 44 de alto.</summary>
        public static RectTransform Create(Transform parent, int columns = 2)
        {
            var go = new GameObject("Chips", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150f, 44f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            go.AddComponent<ResponsiveChipGrid>().Columns = columns;
            return (RectTransform)go.transform;
        }

        private void Awake() => _grid = GetComponent<GridLayoutGroup>();

        // También al cambiar de tamaño: así el primer fotograma ya sale con las celdas buenas.
        private void OnRectTransformDimensionsChange() => Refresh();

        private void LateUpdate() => Refresh();

        private void Refresh()
        {
            if (_grid == null) _grid = GetComponent<GridLayoutGroup>();
            var width = ((RectTransform)transform).rect.width;
            if (_grid == null || width <= 0f || Mathf.Approximately(width, _lastWidth)) return;
            _lastWidth = width;
            _grid.cellSize = new Vector2((width - _grid.spacing.x * (Columns - 1)) / Columns, 44f);
        }
    }
}
