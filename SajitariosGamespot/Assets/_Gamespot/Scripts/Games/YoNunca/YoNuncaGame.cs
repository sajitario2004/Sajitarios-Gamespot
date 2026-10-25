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
    /// <summary>
    /// Tabla <c>yo_nunca_statements</c> y su seed (<c>[{frase, intensidad}]</c>: 98 suaves y 260 picantes +18 desde
    /// V059q). Las frases del usuario (V059q) van en la misma tabla con <c>is_seed = 0</c>.
    /// </summary>
    public static class YoNuncaSchema
    {
        public const string Table = "yo_nunca_statements";
        public const string SeedName = "yo_nunca_statements";

        /// <summary>Última versión de la BD en la que cambió el seed: 7 (picante mucho más fuerte, V059q).</summary>
        public const int SeedRenewedVersion = 7;

        public static void Create(IDb db)
        {
            db.Execute("CREATE TABLE IF NOT EXISTS yo_nunca_statements (id INTEGER PRIMARY KEY AUTOINCREMENT, frase TEXT NOT NULL, " +
                       "intensidad TEXT NOT NULL, is_seed INTEGER NOT NULL DEFAULT 0)");
            db.Execute("CREATE INDEX IF NOT EXISTS idx_yo_nunca_statements_intensidad ON yo_nunca_statements(intensidad)");
        }

        /// <summary>Al migrar desde antes de <see cref="SeedRenewedVersion"/> se borran las de seed; las propias se quedan.</summary>
        public static void Upgrade(IDb db, int oldVersion)
        {
            if (oldVersion < SeedRenewedVersion) db.Execute("DELETE FROM yo_nunca_statements WHERE is_seed = 1");
        }

        /// <summary>Carga el seed si no hay frases de seed (las propias no lo impiden).</summary>
        public static int SeedIfEmpty(IDb db, ISeedSource seeds)
        {
            if (db.Scalar<long>("SELECT COUNT(*) FROM yo_nunca_statements WHERE is_seed = 1") > 0) return 0;
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

    /// <summary>Una frase tal como está en la BD (para la gestión de frases).</summary>
    public sealed class NeverStatementRow
    {
        public long Id { get; set; }
        public string Frase { get; set; }
        public string Intensidad { get; set; }
        public bool IsSeed { get; set; }

        public Intensidad Level => Intensidades.FromStorageName(Intensidad);
    }

    public enum YoNuncaPhraseProblem
    {
        Vacia,
        Duplicada,
        NoExiste,
        Predefinida
    }

    /// <summary>No se pudo guardar o borrar una frase propia.</summary>
    public sealed class YoNuncaPhraseException : Exception
    {
        public YoNuncaPhraseException(YoNuncaPhraseProblem problem) : base($"Frase no válida: {problem}") => Problem = problem;

        public YoNuncaPhraseProblem Problem { get; }
    }

    /// <summary>Qué frases enseña la gestión de frases.</summary>
    public enum YoNuncaPhraseFilter
    {
        Todas,
        Suaves,
        Picantes,
        Propias
    }

    /// <summary>
    /// Frases de Yo Nunca: la fuente de la partida (por niveles y, desde V059q, de serie y/o propias) y la gestión de
    /// las propias: búsqueda, alta, edición y borrado, sin repetir otra frase (sin distinguir mayúsculas ni tildes de
    /// más). Las del seed son de solo lectura.
    /// </summary>
    public sealed class NeverStatementRepository : INeverStatementSource
    {
        private const string Select = "SELECT id AS Id, frase AS Frase, intensidad AS Intensidad, is_seed AS IsSeed FROM yo_nunca_statements";

        private readonly IDb _db;

        public NeverStatementRepository(IDb db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public IReadOnlyList<NeverStatement> GetByIntensidades(IReadOnlyCollection<Intensidad> intensidades) =>
            GetByIntensidades(intensidades, true, true);

        /// <summary>Las frases de esos niveles, de serie y/o propias.</summary>
        public IReadOnlyList<NeverStatement> GetByIntensidades(IReadOnlyCollection<Intensidad> intensidades, bool includeSeed, bool includeOwn)
        {
            if (intensidades == null || intensidades.Count == 0 || (!includeSeed && !includeOwn)) return Array.Empty<NeverStatement>();
            var args = intensidades.Select(i => (object)i.StorageName()).ToList();
            var placeholders = string.Join(", ", args.Select(_ => "?"));
            var origin = includeSeed && includeOwn ? string.Empty : includeSeed ? " AND is_seed = 1" : " AND is_seed = 0";
            return _db.Query<NeverStatementRow>(Select + $" WHERE intensidad IN ({placeholders}){origin} ORDER BY id ASC", args.ToArray())
                .Select(r => new NeverStatement(r.Id, r.Frase, r.Level)).ToList();
        }

        public long CountOwn() => _db.Scalar<long>("SELECT COUNT(*) FROM yo_nunca_statements WHERE is_seed = 0");

        /// <summary>Frases que contienen <paramref name="query"/> (vacío = todas) y pasan el filtro, por orden de alta.</summary>
        public List<NeverStatementRow> Search(string query = null, YoNuncaPhraseFilter filter = YoNuncaPhraseFilter.Todas)
        {
            var q = query?.Trim() ?? string.Empty;
            var filters = new List<string>();
            var args = new List<object>();
            if (q.Length > 0)
            {
                filters.Add("frase LIKE ? ESCAPE '\\'");
                args.Add($"%{q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%");
            }
            switch (filter)
            {
                case YoNuncaPhraseFilter.Suaves:
                    filters.Add("intensidad = ?");
                    args.Add(Intensidad.Suave.StorageName());
                    break;
                case YoNuncaPhraseFilter.Picantes:
                    filters.Add("intensidad = ?");
                    args.Add(Intensidad.Picante.StorageName());
                    break;
                case YoNuncaPhraseFilter.Propias:
                    filters.Add("is_seed = 0");
                    break;
            }
            var where = filters.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", filters);
            // Las propias primero (las más nuevas arriba) y luego las de serie.
            return _db.Query<NeverStatementRow>(Select + where + " ORDER BY is_seed ASC, id DESC", args.ToArray());
        }

        public NeverStatementRow GetById(long id) => _db.Query<NeverStatementRow>(Select + " WHERE id = ?", id).FirstOrDefault();

        /// <summary>Añade una frase propia ya normalizada (<see cref="NeverStatementText.Normalize"/>).</summary>
        public long Insert(string frase, Intensidad intensidad)
        {
            var clean = Check(frase, null);
            _db.Execute("INSERT INTO yo_nunca_statements (frase, intensidad, is_seed) VALUES (?, ?, 0)", clean, intensidad.StorageName());
            return _db.LastInsertRowId;
        }

        public void Update(long id, string frase, Intensidad intensidad)
        {
            var current = GetById(id) ?? throw new YoNuncaPhraseException(YoNuncaPhraseProblem.NoExiste);
            if (current.IsSeed) throw new YoNuncaPhraseException(YoNuncaPhraseProblem.Predefinida);
            var clean = Check(frase, id);
            _db.Execute("UPDATE yo_nunca_statements SET frase = ?, intensidad = ? WHERE id = ? AND is_seed = 0", clean, intensidad.StorageName(), id);
        }

        public void Delete(long id)
        {
            var current = GetById(id) ?? throw new YoNuncaPhraseException(YoNuncaPhraseProblem.NoExiste);
            if (current.IsSeed) throw new YoNuncaPhraseException(YoNuncaPhraseProblem.Predefinida);
            _db.Execute("DELETE FROM yo_nunca_statements WHERE id = ? AND is_seed = 0", id);
        }

        /// <summary>
        /// Recortada y sin repetir otra frase. En C# y no con NOCASE (SQLite solo iguala mayúsculas ASCII); el punto
        /// final no cuenta: «Yo nunca he ido a Ibiza» = «yo nunca he ido a ibiza.».
        /// </summary>
        private string Check(string frase, long? exceptId)
        {
            var clean = NameNormalizer.Normalize(frase);
            if (clean.Length == 0) throw new YoNuncaPhraseException(YoNuncaPhraseProblem.Vacia);
            var key = Key(clean);
            var taken = _db.Query<NeverStatementRow>("SELECT id AS Id, frase AS Frase FROM yo_nunca_statements")
                .Any(r => r.Id != exceptId && Key(r.Frase) == key);
            if (taken) throw new YoNuncaPhraseException(YoNuncaPhraseProblem.Duplicada);
            return clean;
        }

        private static string Key(string frase) => NameNormalizer.DuplicateKey(frase).TrimEnd('.', '!', '…', ' ');
    }

    /// <summary>
    /// Fuente de la partida limitada a las frases de serie y/o a las propias (V059q). El dominio sigue recibiendo una
    /// lista de frases.
    /// </summary>
    public sealed class OriginNeverStatementSource : INeverStatementSource
    {
        private readonly NeverStatementRepository _repository;

        public OriginNeverStatementSource(NeverStatementRepository repository) => _repository = repository;

        public bool IncludeSeed { get; set; } = true;
        public bool IncludeOwn { get; set; } = true;

        public IReadOnlyList<NeverStatement> GetByIntensidades(IReadOnlyCollection<Intensidad> intensidades) =>
            _repository.GetByIntensidades(intensidades, IncludeSeed, IncludeOwn);
    }

    /// <summary>Estado compartido de las pantallas de Yo Nunca.</summary>
    public sealed class YoNuncaModule
    {
        public const string SetupRoute = "/yo-nunca";
        public const string PlayRoute = "/yo-nunca/play";
        public const string PhrasesRoute = "/yo-nunca/frases";

        public YoNuncaModule(YoNuncaFlowController flow, Router router, AudioService audio, NeverStatementRepository phrases,
            OriginNeverStatementSource source)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
            Phrases = phrases;
            Source = source;
        }

        public YoNuncaFlowController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public NeverStatementRepository Phrases { get; }
        public OriginNeverStatementSource Source { get; }
        public HashSet<Intensidad> Draft { get; } = new HashSet<Intensidad> { Intensidad.Suave };

        /// <summary>Frases de serie y propias en la partida (las propias solo cuentan si hay alguna).</summary>
        public bool DraftSeed { get; set; } = true;
        public bool DraftOwn { get; set; } = true;

        /// <summary>
        /// Si ya se confirmó en esta sesión que todos son mayores de edad (para el picante, V059q). Desde V070 es la
        /// confirmación común de todos los juegos (<see cref="AdultGate"/>).
        /// </summary>
        public bool AdultsConfirmed
        {
            get => AdultGate.Confirmed;
            set => AdultGate.Confirmed = value;
        }

        public long OwnCountOrZero()
        {
            try
            {
                return Phrases.CountOwn();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Yo Nunca] No se pudieron contar las frases propias: {e.Message}");
                return 0;
            }
        }

        public void ResetToMenu()
        {
            Flow.Reiniciar();
            Router.Go(Router.Home);
        }
    }

    /// <summary>
    /// Setup: chips Suave (cian, marcado) y Picante +18 (magenta). Al marcar Picante por primera vez se pide confirmar
    /// que todos son mayores de edad (V059q) y mientras está marcado se ve un aviso fijo. Si hay frases propias, se elige
    /// también de dónde salen: de serie y/o mías. La barra abre la gestión de frases.
    /// </summary>
    public sealed class YoNuncaSetupScreen : AppScreen
    {
        private readonly YoNuncaModule _module;
        private NeonChip _suave;
        private NeonChip _picante;
        private NeonChip _seed;
        private NeonChip _own;
        private GameObject _warning;

        public YoNuncaSetupScreen(YoNuncaModule module) => _module = module;

        public bool WarningVisible => _warning != null && _warning.activeSelf;

        /// <summary>Si se ve el panel de origen de las frases (solo con frases propias).</summary>
        public bool OriginVisible => _own != null;

        public NeonChip PicanteChip => _picante;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("yoNuncaTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "yoNuncaTitulo", GameUi.RuleKeys("reglasYoNunca", 4));
            scaffold.AppBar.AddAction(IconId.MenuBook, () => Router.Go(YoNuncaModule.PhrasesRoute)).name = Loc.T("yoNuncaGestionarFrases");
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
            _picante.ToggleOnClick = false;
            _picante.Clicked += () => RequestPicante(!_module.Draft.Contains(Intensidad.Picante));

            var warning = Ui.Panel(content, Palette.NeonMagenta, glow: 0.8f, padding: new RectOffset(16, 16, 14, 14));
            var warningRow = Ui.Row(warning.Content, 12f, alignment: TextAnchor.UpperLeft);
            Ui.Icon(warningRow, IconId.Warning, 28f, Palette.NeonMagenta, 0.8f);
            Ui.Text(warningRow, Loc.T("yoNuncaAdvertenciaPicante"), TextRole.BodyMedium, Palette.TextPrimary).Sized(width: 0f, flexWidth: 1f);
            _warning = warning.gameObject;
            _warning.SetActive(_module.Draft.Contains(Intensidad.Picante));

            _seed = null;
            _own = null;
            if (_module.OwnCountOrZero() > 0)
            {
                var origin = Ui.Panel(content, Palette.NeonViolet.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
                Ui.Text(origin.Content, Loc.T("yoNuncaSetupOrigen"), TextRole.TitleMedium, Palette.NeonVioletText, glow: 0.6f, bold: true);
                Ui.Text(origin.Content, Loc.T("yoNuncaSetupOrigenAyuda"), TextRole.BodySmall, Palette.TextMuted);
                var originRow = Ui.Row(origin.Content, 10f, alignment: TextAnchor.MiddleCenter);
                _seed = NeonChip.Create(originRow, Loc.T("yoNuncaOrigenSerie"), _module.DraftSeed, Palette.NeonViolet);
                _seed.Sized(flexWidth: 1f);
                _seed.ToggleOnClick = false;
                _seed.Clicked += () => SetOrigin(!_module.DraftSeed, _module.DraftOwn);
                _own = NeonChip.Create(originRow, Loc.T("yoNuncaOrigenPropias"), _module.DraftOwn, Palette.NeonViolet);
                _own.Sized(flexWidth: 1f);
                _own.ToggleOnClick = false;
                _own.Clicked += () => SetOrigin(_module.DraftSeed, !_module.DraftOwn);
            }

            Ui.Button(content, Loc.T("yoNuncaEmpezar"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        /// <summary>Marca o desmarca un nivel sin preguntar (también lo usan los tests).</summary>
        public void Toggle(Intensidad intensidad, bool selected)
        {
            if (selected) _module.Draft.Add(intensidad);
            else _module.Draft.Remove(intensidad);
            (intensidad == Intensidad.Suave ? _suave : _picante).Selected = selected;
            _warning.SetActive(_module.Draft.Contains(Intensidad.Picante));
        }

        /// <summary>
        /// Lo que hace el chip de Picante: la primera vez que se marca en la sesión pide confirmar que todos son mayores
        /// de 18 (V059q); si no se confirma, se queda sin marcar.
        /// </summary>
        public void RequestPicante(bool selected)
        {
            if (!selected)
            {
                Toggle(Intensidad.Picante, false);
                return;
            }
            AdultGate.Request(() => Toggle(Intensidad.Picante, true), "yoNuncaConfirmarPicanteTitulo", "yoNuncaConfirmarPicanteMensaje",
                "yoNuncaConfirmarPicanteSi");
        }

        /// <summary>De dónde salen las frases; siempre queda al menos uno.</summary>
        public void SetOrigin(bool seed, bool own)
        {
            if (!seed && !own) return;
            _module.DraftSeed = seed;
            _module.DraftOwn = own;
            if (_seed != null) _seed.Selected = seed;
            if (_own != null) _own.Selected = own;
        }

        public void Start()
        {
            var config = YoNuncaConfig.Create(_module.Draft);
            if (config == null)
            {
                Snackbar.Show(Loc.T("yoNuncaErrorSinIntensidades"), Palette.NeonError);
                return;
            }
            // Sin frases propias no se puede elegir: salen las de serie.
            var hasOwn = _own != null;
            _module.Source.IncludeSeed = !hasOwn || _module.DraftSeed;
            _module.Source.IncludeOwn = hasOwn && _module.DraftOwn;
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
            // Las picantes, en magenta (V059q): se ve a la primera qué nivel ha salido.
            var spicy = current != null && current.Intensidad == Intensidad.Picante;
            _panel.SetColors(spicy ? Palette.NeonMagenta : Palette.NeonCyan);
            Ui.SetGlow((TextMeshProUGUI)_statement, spicy ? Palette.NeonMagenta : Palette.NeonCyan, 1f);
        }
    }

    /// <summary>
    /// Descriptor de Yo Nunca: frases suaves y picantes (+18), sin repetir hasta agotarlas, y desde V059q también las
    /// del usuario.
    /// </summary>
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
            var phrases = new NeverStatementRepository(context.Db);
            var source = new OriginNeverStatementSource(phrases);
            var module = new YoNuncaModule(new YoNuncaFlowController(source, context.Random), context.Router, context.Audio, phrases, source);
            Module = module;
            context.Router.Register(YoNuncaModule.SetupRoute, () => new YoNuncaSetupScreen(module));
            context.Router.Register(YoNuncaModule.PlayRoute, () => new YoNuncaPlayScreen(module));
            context.Router.Register(YoNuncaModule.PhrasesRoute, () => new YoNuncaPhrasesScreen(module));
        }

        public void OnCreate(IDb db) => YoNuncaSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion) => YoNuncaSchema.Upgrade(db, oldVersion);

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => YoNuncaSchema.SeedIfEmpty(db, seeds);
    }
}
