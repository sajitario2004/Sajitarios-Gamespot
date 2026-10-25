using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Configuración de la partida (port de <c>SetupScreen</c>): 3–15 jugadores en orden de revelación, impostores
    /// 1..min(5, n−1), rondas de votación 1..max(1, n−3) y pista. Las rondas empiezan en el máximo y lo siguen al
    /// añadir o quitar jugadores hasta que se tocan; luego solo se recortan (I1; en el Flutter empezaban en 1).
    /// Categorías (V059i): la palabra sale solo de las marcadas, también de las propias (V059l). Al empezar valida con
    /// <see cref="GameConfig.Create"/> y reparte roles.
    /// </summary>
    public sealed class ImpostorSetupScreen : AppScreen
    {
        private readonly ImpostorModule _module;
        private PlayerListEditor _players;
        private NeonStepper _impostores;
        private NeonStepper _rounds;
        private CategoryPicker _categories;

        public ImpostorSetupScreen(ImpostorModule module) => _module = module;

        public PlayerListEditor Players => _players;
        public NeonStepper ImpostorsStepper => _impostores;
        public NeonStepper RoundsStepper => _rounds;
        public CategoryPicker Categories => _categories;

        public override void Build(RectTransform root)
        {
            var draft = _module.Draft;
            var scaffold = ScreenScaffold.Create(root, Loc.T("impostorTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "impostorTitulo", GameUi.RuleKeys("reglasImpostor", 6));
            scaffold.AppBar.AddAction(IconId.History, () => Router.Go(ImpostorModule.HistoryRoute)).name = Loc.T("historial");
            scaffold.AppBar.AddAction(IconId.MenuBook, () => Router.Go(ImpostorModule.WordsRoute)).name = Loc.T("gestionarPalabras");

            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));

            var playersPanel = Ui.Panel(content, Palette.NeonCyan.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(playersPanel.Content, Loc.T("setupJugadores"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            Ui.Text(playersPanel.Content, Loc.T("setupRangoJugadores", ("min", GameConfig.MinPlayers), ("max", GameConfig.MaxPlayers)),
                TextRole.BodySmall, Palette.TextMuted);
            _players = PlayerListEditor.Create(playersPanel.Content, GameConfig.MinPlayers, GameConfig.MaxPlayers, Palette.NeonCyan, draft.Names);
            _players.CountChanged += OnPlayersChanged;
            _players.NamesChanged += SaveDraft;

            var impostorsPanel = Ui.Panel(content, Palette.NeonMagenta.WithAlpha(0.5f), glow: 0.3f, spacing: 6f);
            Ui.Text(impostorsPanel.Content, Loc.T("setupImpostores"), TextRole.TitleMedium, Palette.NeonMagenta, glow: 0.6f, bold: true);
            _impostores = NeonStepper.Create(impostorsPanel.Content, draft.Impostores, GameConfig.MinImpostores,
                GameConfig.MaxImpostoresFor(_players.Count), n => Loc.T("impostoresContador", ("n", n)), Palette.NeonMagenta);
            _impostores.SetButtonNames(Loc.T("menosImpostores"), Loc.T("masImpostores"));
            _impostores.Changed += _ => SaveDraft();

            var roundsPanel = Ui.Panel(content, Palette.NeonViolet.WithAlpha(0.5f), glow: 0.3f, spacing: 6f);
            Ui.Text(roundsPanel.Content, Loc.T("setupRondas"), TextRole.TitleMedium, Palette.NeonVioletText, glow: 0.6f, bold: true);
            _rounds = NeonStepper.Create(roundsPanel.Content, draft.Rounds, GameConfig.MinRounds, GameConfig.MaxRoundsFor(_players.Count),
                n => Loc.T("rondasContador", ("n", n)), Palette.NeonViolet);
            _rounds.SetButtonNames(Loc.T("menosRondas"), Loc.T("masRondas"));
            _rounds.Changed += _ =>
            {
                _module.Draft.RoundsTouched = true;
                SaveDraft();
            };

            var hint = NeonSwitch.Create(content, Loc.T("darPistaAlImpostor"), Loc.T("darPistaAlImpostorSubtitulo"), draft.HintEnabled);
            hint.Changed += v =>
            {
                _module.Draft.HintEnabled = v;
            };

            _categories = CategoryPicker.Create(content, AvailableCategories(), draft.Categories, Palette.NeonCyan, draft.KnownCategories);

            Ui.Space(content, 8f);
            Ui.Button(content, Loc.T("empezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
            OnPlayersChanged();
        }

        /// <summary>
        /// Categorías con alguna palabra, en el orden de los chips: las de serie, "Mis palabras" y las propias (solo
        /// las que tienen palabras).
        /// </summary>
        private IReadOnlyList<CategoryOption> AvailableCategories()
        {
            var custom = _module.CustomCategoriesOrEmpty();
            try
            {
                var counts = _module.Words.CountByCategory();
                return CategoryOption.For(CategoryOption.AllIds(custom).Where(id => counts.TryGetValue(id, out var n) && n > 0), custom);
            }
            catch (System.Exception)
            {
                return CategoryOption.For(WordCategories.Seed, custom);
            }
        }

        /// <summary>
        /// Al añadir o quitar jugadores los máximos cambian: los impostores solo se recortan; las rondas siguen al
        /// máximo mientras no se hayan tocado.
        /// </summary>
        private void OnPlayersChanged()
        {
            var n = _players.Count;
            _impostores.SetRange(GameConfig.MinImpostores, GameConfig.MaxImpostoresFor(n));
            _impostores.SetHelper(Loc.T("maximoImpostoresPara", ("max", GameConfig.MaxImpostoresFor(n)), ("jugadores", n)));
            var maxRounds = GameConfig.MaxRoundsFor(n);
            _rounds.SetRange(GameConfig.MinRounds, maxRounds, _module.Draft.RoundsTouched ? (int?)null : maxRounds);
            _rounds.SetHelper(Loc.T("setupRondasAyuda", ("min", GameConfig.MinRounds), ("max", GameConfig.MaxRoundsFor(n))));
            SaveDraft();
        }

        private void SaveDraft()
        {
            var draft = _module.Draft;
            draft.Names.Clear();
            draft.Names.AddRange(_players.RawNames);
            draft.Impostores = _impostores?.Value ?? draft.Impostores;
            draft.Rounds = _rounds?.Value ?? draft.Rounds;
        }

        /// <summary>"Empezar partida" (también lo usan los tests).</summary>
        public void Start()
        {
            SaveDraft();
            var players = _players.NormalizedNames.Select(n => new Player(n)).ToList();
            var result = GameConfig.Create(players, _impostores.Value, _module.Draft.HintEnabled, _rounds.Value);
            if (!result.IsSuccess)
            {
                Snackbar.Show(ErrorText(result.Error.Value), Palette.NeonError);
                return;
            }

            _players.Remember();
            _module.WordSource.Categories = _categories.Selected;
            _module.Flow.Iniciar(result.Config);
            switch (_module.Flow.Phase)
            {
                case ImpostorPhase.Pass:
                    Router.Go(ImpostorModule.PassRoute);
                    break;
                case ImpostorPhase.Error when _module.Flow.ErrorKind == ImpostorErrorKind.SinPalabras:
                    Dialogs.Show(Loc.T("noHayPalabrasTitulo"), Loc.T("noHayPalabrasMensaje"), new[]
                    {
                        new DialogAction(Loc.T("ahoraNo"), (System.Action)null, ButtonStyle.Text, Palette.NeonCyan),
                        new DialogAction(Loc.T("gestionarPalabras"), () => Router.Go(ImpostorModule.WordsRoute), ButtonStyle.Filled, Palette.NeonCyan)
                    });
                    break;
                default:
                    Snackbar.Show(Loc.T("errorNoSePudoIniciar"), Palette.NeonError);
                    break;
            }
        }

        private static string ErrorText(GameConfigError error)
        {
            switch (error)
            {
                case GameConfigError.PocosJugadores: return Loc.T("errorPocosJugadores", ("min", GameConfig.MinPlayers));
                case GameConfigError.DemasiadosJugadores: return Loc.T("errorDemasiadosJugadores", ("max", GameConfig.MaxPlayers));
                case GameConfigError.NombreVacio: return Loc.T("errorNombreVacio");
                default: return Loc.T("errorNombresDuplicados");
            }
        }
    }
}
