using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.YoNunca.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.YoNunca
{
    /// <summary>Tabla <c>yo_nunca_statements</c> y su seed (<c>[{frase, intensidad}]</c>, 98 suaves y 94 picantes).</summary>
    public static class YoNuncaSchema
    {
        public const string Table = "yo_nunca_statements";
        public const string SeedName = "yo_nunca_statements";

        public static void Create(IDb db)
        {
            db.Execute("CREATE TABLE IF NOT EXISTS yo_nunca_statements (id INTEGER PRIMARY KEY AUTOINCREMENT, frase TEXT NOT NULL, " +
                       "intensidad TEXT NOT NULL, is_seed INTEGER NOT NULL DEFAULT 0)");
            db.Execute("CREATE INDEX IF NOT EXISTS idx_yo_nunca_statements_intensidad ON yo_nunca_statements(intensidad)");
        }

        public static int SeedIfEmpty(IDb db, ISeedSource seeds)
        {
            if (!AppDatabase.IsEmpty(db, Table)) return 0;
            var inserted = 0;
            foreach (var s in Parse(seeds.Load(SeedName)))
            {
                inserted += db.Execute("INSERT INTO yo_nunca_statements (frase, intensidad, is_seed) VALUES (?, ?, 1)", s.Frase, s.Intensidad.StorageName());
            }
            return inserted;
        }

        public static List<NeverStatement> Parse(string json)
        {
            var result = new List<NeverStatement>();
            foreach (var item in Json.ParseArray(json))
            {
                if (!(item is Dictionary<string, object> map) || !(map.TryGetValue("frase", out var f) && f is string frase)
                    || !(map.TryGetValue("intensidad", out var i) && i is string intensidad))
                {
                    throw new FormatException("Frase de Yo Nunca sin 'frase' o 'intensidad'");
                }
                result.Add(new NeverStatement(0, frase, Intensidades.FromStorageName(intensidad)));
            }
            return result;
        }
    }

    /// <summary>Frases por niveles, por id.</summary>
    public sealed class NeverStatementRepository : INeverStatementSource
    {
        private sealed class Row
        {
            public long Id { get; set; }
            public string Frase { get; set; }
            public string Intensidad { get; set; }
        }

        private readonly IDb _db;

        public NeverStatementRepository(IDb db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public IReadOnlyList<NeverStatement> GetByIntensidades(IReadOnlyCollection<Intensidad> intensidades)
        {
            if (intensidades == null || intensidades.Count == 0) return Array.Empty<NeverStatement>();
            var names = intensidades.Select(i => (object)i.StorageName()).ToArray();
            var placeholders = string.Join(", ", names.Select(_ => "?"));
            return _db.Query<Row>($"SELECT id AS Id, frase AS Frase, intensidad AS Intensidad FROM yo_nunca_statements WHERE intensidad IN ({placeholders}) ORDER BY id ASC", names)
                .Select(r => new NeverStatement(r.Id, r.Frase, Intensidades.FromStorageName(r.Intensidad))).ToList();
        }
    }

    /// <summary>Estado compartido de las pantallas de Yo Nunca.</summary>
    public sealed class YoNuncaModule
    {
        public const string SetupRoute = "/yo-nunca";
        public const string PlayRoute = "/yo-nunca/play";

        public YoNuncaModule(YoNuncaFlowController flow, Router router, AudioService audio)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
        }

        public YoNuncaFlowController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public HashSet<Intensidad> Draft { get; } = new HashSet<Intensidad> { Intensidad.Suave };

        public void ResetToMenu()
        {
            Flow.Reiniciar();
            Router.Go(Router.Home);
        }
    }

    /// <summary>
    /// Setup: chips Suave (cian, marcado) y Picante (magenta). Con Picante aparece un aviso fijo: contenido sexual
    /// explícito, solo si todos son mayores de 18 y dan su consentimiento.
    /// </summary>
    public sealed class YoNuncaSetupScreen : AppScreen
    {
        private readonly YoNuncaModule _module;
        private NeonChip _suave;
        private NeonChip _picante;
        private GameObject _warning;

        public YoNuncaSetupScreen(YoNuncaModule module) => _module = module;

        public bool WarningVisible => _warning != null && _warning.activeSelf;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("yoNuncaTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "yoNuncaTitulo", GameUi.RuleKeys("reglasYoNunca", 4));
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));

            var panel = Ui.Panel(content, Palette.NeonCyan.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(panel.Content, Loc.T("yoNuncaSetupIntensidades"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            Ui.Text(panel.Content, Loc.T("yoNuncaSetupIntensidadesAyuda"), TextRole.BodySmall, Palette.TextMuted);
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleCenter);
            _suave = NeonChip.Create(row, Loc.T("yoNuncaIntensidadSuave"), _module.Draft.Contains(Intensidad.Suave), Palette.NeonCyan);
            _suave.Sized(flexWidth: 1f);
            _suave.Changed += v => Toggle(Intensidad.Suave, v);
            _picante = NeonChip.Create(row, Loc.T("yoNuncaIntensidadPicante"), _module.Draft.Contains(Intensidad.Picante), Palette.NeonMagenta);
            _picante.Sized(flexWidth: 1f);
            _picante.Changed += v => Toggle(Intensidad.Picante, v);

            var warning = Ui.Panel(content, Palette.NeonMagenta, glow: 0.8f, padding: new RectOffset(16, 16, 14, 14));
            var warningRow = Ui.Row(warning.Content, 12f, alignment: TextAnchor.UpperLeft);
            Ui.Icon(warningRow, IconId.Warning, 28f, Palette.NeonMagenta, 0.8f);
            Ui.Text(warningRow, Loc.T("yoNuncaAdvertenciaPicante"), TextRole.BodyMedium, Palette.TextPrimary).Sized(flexWidth: 1f);
            _warning = warning.gameObject;
            _warning.SetActive(_module.Draft.Contains(Intensidad.Picante));

            Ui.Button(content, Loc.T("yoNuncaEmpezar"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        public void Toggle(Intensidad intensidad, bool selected)
        {
            if (selected) _module.Draft.Add(intensidad);
            else _module.Draft.Remove(intensidad);
            (intensidad == Intensidad.Suave ? _suave : _picante).Selected = selected;
            _warning.SetActive(_module.Draft.Contains(Intensidad.Picante));
        }

        public void Start()
        {
            var config = YoNuncaConfig.Create(_module.Draft);
            if (config == null)
            {
                Snackbar.Show(Loc.T("yoNuncaErrorSinIntensidades"), Palette.NeonError);
                return;
            }
            _module.Flow.Iniciar(config);
            if (_module.Flow.Fase == YoNuncaFase.Jugando)
            {
                Router.Go(YoNuncaModule.PlayRoute);
                return;
            }
            Dialogs.Show(Loc.T("yoNuncaSinFrasesTitulo"), Loc.T("yoNuncaSinFrasesMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            });
        }
    }

    /// <summary>Juego: la frase en grande y "Siguiente". Casa y atrás piden confirmación (en el Flutter, atrás salía sin preguntar).</summary>
    public sealed class YoNuncaPlayScreen : AppScreen
    {
        private readonly YoNuncaModule _module;
        private TMP_Text _statement;
        private NeonPanel _panel;

        public YoNuncaPlayScreen(YoNuncaModule module) => _module = module;

        public string Statement => _statement != null ? _statement.text : null;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("yoNuncaTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router, () => _module.Flow.Reiniciar());
            var body = Ui.Column(scaffold.Body, 16f, new RectOffset(20, 20, 16, 20));
            body.Sized(flexHeight: 1f);
            if (_module.Flow.Current == null)
            {
                Ui.Text(body, Loc.T("yoNuncaNoHaySesion"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(body, Loc.T("configurarPartida"), () => Router.Go(YoNuncaModule.SetupRoute));
                return;
            }
            // Sin scroll: las frases son cortas y la máscara del scroll recortaba el halo. Centrada en vertical.
            Ui.Flex(body);
            _panel = Ui.Panel(body, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 40, 40), alignment: TextAnchor.MiddleCenter);
            PulseGlow.On(_panel, 0.8f, 1.2f);
            _statement = Ui.Text(_panel.Content, string.Empty, TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 1f,
                Palette.NeonCyan, bold: true);
            _statement.lineSpacing = 10f;
            Ui.Flex(body);
            Ui.Button(body, Loc.T("yoNuncaSiguiente"), Next, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f).Sized(height: 64f);
            Refresh();
        }

        public void Next()
        {
            _module.Flow.Siguiente();
            _module.Audio?.Play(AppSound.Swipe, 0.6f);
            Refresh();
            FadeScaleIn.Play(_panel.gameObject, 0.25f, 0.96f);
        }

        public override bool OnBack()
        {
            if (_module.Flow.Current == null) return false;
            AbandonGameDialog.Show(_module.ResetToMenu);
            return true;
        }

        private void Refresh()
        {
            var current = _module.Flow.Current;
            _statement.text = current?.Frase ?? string.Empty;
            _panel.name = Loc.T("yoNuncaFraseSemantica", ("frase", _statement.text));
        }
    }

    /// <summary>Descriptor de Yo Nunca: frases suaves y picantes (+18), sin repetir hasta agotarlas.</summary>
    public sealed class YoNuncaGame : IGameDescriptor
    {
        public string Id => "yo_nunca";
        public string TitleKey => "yoNuncaMenuTitulo";
        public string DescriptionKey => "yoNuncaMenuDescripcion";
        public IconId Icon => IconId.FrontHand;
        public string EntryRoute => YoNuncaModule.SetupRoute;

        public YoNuncaModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var module = new YoNuncaModule(new YoNuncaFlowController(new NeverStatementRepository(context.Db), context.Random), context.Router, context.Audio);
            Module = module;
            context.Router.Register(YoNuncaModule.SetupRoute, () => new YoNuncaSetupScreen(module));
            context.Router.Register(YoNuncaModule.PlayRoute, () => new YoNuncaPlayScreen(module));
        }

        public void OnCreate(IDb db) => YoNuncaSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => YoNuncaSchema.SeedIfEmpty(db, seeds);
    }
}
