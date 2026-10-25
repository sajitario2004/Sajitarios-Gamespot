using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Menu;
using UnityEngine;

namespace Gamespot.App
{
    /// <summary>
    /// Punto de entrada de la app (composition root): vive en el objeto <c>AppRoot</c> de la escena <c>App</c>.
    /// Carga el idioma, monta la UI, abre la base de datos con el esquema y los seeds de todos los juegos,
    /// precarga el audio, registra las rutas y muestra el menú.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        /// <summary>Ruta de BD alternativa para los tests PlayMode (nula = la del dispositivo).</summary>
        public static string DatabasePathOverride { get; set; }

        /// <summary>Juegos alternativos para los tests (nulo = <see cref="GameRegistry"/>).</summary>
        public static Func<IReadOnlyList<IGameDescriptor>> GamesOverride { get; set; }

        public UiRoot Ui { get; private set; }
        public Router Router { get; private set; }
        public SqliteDb Database { get; private set; }
        public AudioService Audio { get; private set; }
        public IdleRendering Idle { get; private set; }
        public IReadOnlyList<IGameDescriptor> Games { get; private set; }

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            Loc.Initialize();

            Ui = UiRoot.Create(Camera.main);
            Dialogs.Initialize(Ui.OverlayLayer);
            Snackbar.Initialize(Ui.OverlayLayer);

            Games = GamesOverride?.Invoke() ?? GameRegistry.CreateAll();
            Database = AppDatabase.Open(DatabasePathOverride ?? AppDatabase.DefaultPath, Games, new ResourcesSeedSource());

            Audio = AudioService.Create(gameObject);
            Audio.Preload();

            Router = Router.Create(gameObject, Ui.ScreenLayer, path => new NotFoundScreen(path));
            // En reposo se dibuja a 30 fps; al tocar o al cambiar de pantalla, a 60 (V063).
            Idle = IdleRendering.Create(gameObject);
            Router.Navigated += _ => Idle.Wake();
            Router.Register(Router.Home, () => new MenuScreen(Games, Audio));
            var context = new GameContext(Router, Database, new SeededRandom(), Audio);
            foreach (var game in Games) game.Register(context);

            Router.Go(Router.Home);
        }

        private void OnDestroy()
        {
            Database?.Dispose();
            Database = null;
            if (Ui != null) Destroy(Ui.gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            DatabasePathOverride = null;
            GamesOverride = null;
        }
    }
}
