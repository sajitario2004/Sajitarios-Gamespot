using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gamespot.App;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gamespot.Tests.PlayMode
{
    /// <summary>
    /// Arranca la app completa (Bootstrap) con una base de datos temporal y, opcionalmente, juegos de prueba.
    /// Se destruye todo al terminar.
    /// </summary>
    public sealed class AppTestHarness : IDisposable
    {
        private readonly string _dbPath;
        private readonly IReadOnlyList<string> _savedLastPlayers;
        private GameObject _root;

        public AppTestHarness(Func<IReadOnlyList<IGameDescriptor>> games = null)
        {
            // Los jugadores guardados se comparten entre juegos: cada test empieza sin ninguno y deja los de antes.
            _savedLastPlayers = AppSettings.LastPlayers;
            AppSettings.LastPlayers = null;
            _dbPath = Path.Combine(Path.GetTempPath(), $"gamespot_play_{Guid.NewGuid():N}.db");
            Bootstrap.DatabasePathOverride = _dbPath;
            Bootstrap.GamesOverride = games;
            _root = new GameObject("AppRoot (test)");
            Bootstrap = _root.AddComponent<Bootstrap>();
            Bootstrap.Router.QuitAction = () => QuitRequested = true;
        }

        public Bootstrap Bootstrap { get; }
        public Router Router => Bootstrap.Router;
        public bool QuitRequested { get; private set; }

        /// <summary>Espera un par de fotogramas para que el layout se calcule.</summary>
        public static IEnumerator Frames(int count = 2)
        {
            for (var i = 0; i < count; i++) yield return null;
        }

        public void Dispose()
        {
            Dialogs.CloseAll();
            if (_root != null) Object.DestroyImmediate(_root);
            _root = null;
            Bootstrap.DatabasePathOverride = null;
            Bootstrap.GamesOverride = null;
            AppSettings.LastPlayers = _savedLastPlayers;
            try
            {
                if (File.Exists(_dbPath)) File.Delete(_dbPath);
            }
            catch (IOException)
            {
                // Se queda en la carpeta temporal.
            }
        }
    }

    /// <summary>Juego de prueba sin tablas, con una pantalla de entrada vacía.</summary>
    public sealed class FakeGame : IGameDescriptor
    {
        public FakeGame(string id) => Id = id;

        public string Id { get; }
        public string TitleKey => "appTitle";
        public string DescriptionKey => "menuVacioMensaje";
        public IconId Icon => IconId.Casino;
        public string EntryRoute => $"/{Id}";

        public void Register(GameContext context) => context.Router.Register(EntryRoute, () => new EmptyScreen());

        public void OnCreate(IDb db)
        {
        }

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds)
        {
        }

        private sealed class EmptyScreen : AppScreen
        {
            public override void Build(RectTransform root)
            {
            }
        }
    }
}
