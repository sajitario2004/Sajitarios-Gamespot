using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Lista con scroll que solo crea las filas visibles (como <c>ListView.builder</c>): filas de alto fijo que se
    /// reciclan al desplazarse. Para listas largas (las 2060 palabras del Impostor).
    /// </summary>
    public sealed class VirtualList : MonoBehaviour
    {
        private ScrollRect _scroll;
        private RectTransform _content;
        private readonly List<RectTransform> _pool = new List<RectTransform>();
        private readonly Dictionary<int, RectTransform> _visible = new Dictionary<int, RectTransform>();
        private Func<RectTransform, RectTransform> _createRow;
        private Action<RectTransform, int> _bindRow;
        private float _rowHeight;
        private float _spacing;
        private RectOffset _padding;
        private int _count;

        public int Count => _count;

        /// <summary>Filas que existen ahora mismo (visibles + recicladas).</summary>
        public int CreatedRows => _pool.Count + _visible.Count;

        /// <summary>
        /// <paramref name="createRow"/> crea una fila vacía bajo el contenedor recibido; <paramref name="bindRow"/>
        /// la rellena con el elemento del índice. Ocupa el espacio sobrante del padre.
        /// </summary>
        public static VirtualList Create(Transform parent, float rowHeight, Func<RectTransform, RectTransform> createRow,
            Action<RectTransform, int> bindRow, float spacing = 8f, RectOffset padding = null)
        {
            var go = new GameObject("VirtualList", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().Sized(flexWidth: 1f, flexHeight: 1f);
            var list = go.AddComponent<VirtualList>();
            list._rowHeight = rowHeight;
            list._spacing = spacing;
            list._padding = padding ?? new RectOffset(16, 16, 8, 96);
            list._createRow = createRow;
            list._bindRow = bindRow;

            list._scroll = go.AddComponent<ScrollRect>();
            list._scroll.horizontal = false;
            list._scroll.movementType = ScrollRect.MovementType.Elastic;
            list._scroll.scrollSensitivity = 30f;
            var viewport = UiLayout.CreateStretched("Viewport", go.transform);
            viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport, false);
            list._content = (RectTransform)content.transform;
            list._content.anchorMin = new Vector2(0f, 1f);
            list._content.anchorMax = new Vector2(1f, 1f);
            list._content.pivot = new Vector2(0.5f, 1f);
            list._content.offsetMin = Vector2.zero;
            list._content.offsetMax = Vector2.zero;
            list._scroll.viewport = viewport;
            list._scroll.content = list._content;
            list._scroll.onValueChanged.AddListener(_ => list._dirty = true);
            return list;
        }

        // Las filas se reciclan en LateUpdate: activar o desactivar gráficos dentro del ciclo de layout de uGUI
        // (OnRectTransformDimensionsChange) da error ("inside a rebuild loop").
        private bool _dirty = true;

        /// <summary>Cambia el número de elementos y vuelve a pintar las filas visibles.</summary>
        public void SetCount(int count, bool scrollToTop = false)
        {
            _count = Math.Max(0, count);
            _content.sizeDelta = new Vector2(0f, _padding.top + _padding.bottom + _count * (_rowHeight + _spacing));
            if (scrollToTop) _content.anchoredPosition = Vector2.zero;
            foreach (var pair in _visible) Release(pair.Value);
            _visible.Clear();
            _dirty = true;
        }

        private void OnRectTransformDimensionsChange() => _dirty = true;

        private void LateUpdate()
        {
            if (!_dirty) return;
            _dirty = false;
            Refresh();
        }

        /// <summary>Pinta ya las filas visibles (normalmente se hace en LateUpdate).</summary>
        public void Refresh()
        {
            if (_scroll == null || _content == null) return;
            var viewportHeight = ((RectTransform)_scroll.viewport).rect.height;
            if (viewportHeight <= 0f) return;
            var top = Mathf.Max(0f, _content.anchoredPosition.y - _padding.top);
            var stride = _rowHeight + _spacing;
            var first = Mathf.Clamp(Mathf.FloorToInt(top / stride) - 1, 0, Mathf.Max(0, _count - 1));
            var last = Mathf.Clamp(Mathf.CeilToInt((top + viewportHeight) / stride) + 1, 0, _count - 1);

            var toRemove = new List<int>();
            foreach (var pair in _visible)
            {
                if (pair.Key < first || pair.Key > last) toRemove.Add(pair.Key);
            }
            foreach (var index in toRemove)
            {
                Release(_visible[index]);
                _visible.Remove(index);
            }

            for (var i = first; i <= last && _count > 0; i++)
            {
                if (_visible.ContainsKey(i)) continue;
                var row = Acquire();
                row.anchorMin = new Vector2(0f, 1f);
                row.anchorMax = new Vector2(1f, 1f);
                row.pivot = new Vector2(0.5f, 1f);
                row.offsetMin = new Vector2(_padding.left, 0f);
                row.offsetMax = new Vector2(-_padding.right, 0f);
                row.sizeDelta = new Vector2(row.sizeDelta.x, _rowHeight);
                row.anchoredPosition = new Vector2(row.anchoredPosition.x, -(_padding.top + i * stride));
                _bindRow(row, i);
                _visible[i] = row;
            }
        }

        private RectTransform Acquire()
        {
            if (_pool.Count > 0)
            {
                var row = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
                row.gameObject.SetActive(true);
                return row;
            }
            return _createRow(_content);
        }

        private void Release(RectTransform row)
        {
            row.gameObject.SetActive(false);
            _pool.Add(row);
        }
    }
}
