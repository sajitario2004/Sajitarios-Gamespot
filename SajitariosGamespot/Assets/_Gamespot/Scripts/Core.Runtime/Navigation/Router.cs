using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gamespot.Core.Runtime.Navigation
{
    /// <summary>
    /// Navegación por rutas de texto (port de go_router): <c>/</c> es el menú y cada juego registra las suyas
    /// (<c>/impostor</c>, <c>/impostor/pass</c>…). <see cref="Go"/> sustituye la pantalla; <see cref="Push"/>
    /// apila una encima (reglas) y <see cref="Pop"/> la quita. El botón atrás de Android (Escape) cierra el
    /// diálogo abierto, o lo pasa a la pantalla, o vuelve a la ruta padre.
    /// </summary>
    public sealed class Router : MonoBehaviour
    {
        public const string Home = "/";

        private readonly Dictionary<string, Func<AppScreen>> _routes = new Dictionary<string, Func<AppScreen>>();
        private readonly List<AppScreen> _stack = new List<AppScreen>();
        private RectTransform _screenLayer;
        private Func<string, AppScreen> _notFound;

        /// <summary>Ruta de la pantalla base visible.</summary>
        public string CurrentPath { get; private set; }

        /// <summary>Pantalla visible (la de arriba de la pila).</summary>
        public AppScreen Current => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

        /// <summary>Se lanza tras cada cambio de pantalla.</summary>
        public event Action<string> Navigated;

        /// <summary>Acción del botón atrás en el menú (por defecto, salir de la app).</summary>
        public Action QuitAction { get; set; } = Application.Quit;

        public static Router Create(GameObject host, RectTransform screenLayer, Func<string, AppScreen> notFound)
        {
            var router = host.AddComponent<Router>();
            router._screenLayer = screenLayer;
            router._notFound = notFound;
            return router;
        }

        public void Register(string path, Func<AppScreen> factory)
        {
            if (string.IsNullOrEmpty(path) || path[0] != '/') throw new ArgumentException($"Ruta no válida: {path}", nameof(path));
            if (_routes.ContainsKey(path)) throw new InvalidOperationException($"Ruta repetida: {path}");
            _routes[path] = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public bool HasRoute(string path) => _routes.ContainsKey(path);

        /// <summary>Muestra la ruta <paramref name="path"/> sustituyendo todo lo que hubiera.</summary>
        public void Go(string path)
        {
            Dialogs.CloseAll();
            while (_stack.Count > 0) RemoveTop();
            CurrentPath = path;
            var screen = _routes.TryGetValue(path, out var factory) ? factory() : _notFound?.Invoke(path);
            if (screen == null) throw new InvalidOperationException($"No hay pantalla para {path}");
            Show(screen, path);
            Navigated?.Invoke(path);
        }

        /// <summary>Apila una pantalla encima de la actual (la de abajo se oculta, no se destruye).</summary>
        public void Push(AppScreen screen)
        {
            if (Current != null) Current.Root.gameObject.SetActive(false);
            Show(screen, null);
            Navigated?.Invoke(CurrentPath);
        }

        /// <summary>Quita la pantalla apilada de arriba. Con una sola pantalla no hace nada.</summary>
        public void Pop()
        {
            if (_stack.Count <= 1) return;
            RemoveTop();
            Current.Root.gameObject.SetActive(true);
            Navigated?.Invoke(CurrentPath);
        }

        /// <summary>Botón atrás: diálogo → pantalla → pila → ruta padre → salir.</summary>
        public void Back()
        {
            if (Dialogs.HandleBack()) return;
            var current = Current;
            if (current != null && current.OnBack()) return;
            if (_stack.Count > 1)
            {
                Pop();
                return;
            }
            if (CurrentPath == Home || string.IsNullOrEmpty(CurrentPath))
            {
                QuitAction?.Invoke();
                return;
            }
            Go(ParentOf(CurrentPath));
        }

        /// <summary>Ruta padre: <c>/impostor/pass</c> → <c>/impostor</c>; <c>/impostor</c> → <c>/</c>.</summary>
        public static string ParentOf(string path)
        {
            if (string.IsNullOrEmpty(path) || path == Home) return Home;
            var index = path.LastIndexOf('/');
            return index <= 0 ? Home : path.Substring(0, index);
        }

        /// <summary>Reconstruye las pantallas (cambio de idioma).</summary>
        public void RebuildAll()
        {
            foreach (var screen in _stack)
            {
                screen.Root.ClearChildren();
                screen.Build(screen.Root);
            }
        }

        private void OnEnable() => Loc.LanguageChanged += RebuildAll;

        private void OnDisable() => Loc.LanguageChanged -= RebuildAll;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Back();
            Current?.Tick(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            while (_stack.Count > 0) RemoveTop();
        }

        private void Show(AppScreen screen, string path)
        {
            var root = UiLayout.CreateStretched(screen.GetType().Name, _screenLayer);
            screen.Root = root;
            screen.Router = this;
            screen.Path = path;
            _stack.Add(screen);
            screen.Build(root);
            screen.OnShow();
        }

        private void RemoveTop()
        {
            var top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            try
            {
                top.OnHide();
            }
            finally
            {
                if (top.Root != null)
                {
                    top.Root.gameObject.SetActive(false);
                    Destroy(top.Root.gameObject);
                }
            }
        }
    }
}
