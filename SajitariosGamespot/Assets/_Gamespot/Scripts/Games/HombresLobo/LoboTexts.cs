using System.Collections.Generic;
using System.Linq;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Games.HombresLobo.Domain;

namespace Gamespot.Games.HombresLobo
{
    /// <summary>Textos de la consola del game master: los pasos de la noche, sus opciones y el registro (V079).</summary>
    public static class LoboTexts
    {
        public static string StepTitle(LoboPaso paso) => Loc.T("loboPasoTitulo" + paso);

        /// <summary>Lo que lee el game master en voz alta.</summary>
        public static string Narration(LoboPaso paso) => Loc.T("loboNarrar" + paso);

        /// <summary>Lo que tiene que hacer el game master en el móvil.</summary>
        public static string Hint(LoboPaso paso) => Loc.T("loboAyudaPaso" + paso);

        public static string PlayerName(LoboGame game, int? index) =>
            index.HasValue && index.Value >= 0 && index.Value < game.Jugadores.Count ? game.Jugadores[index.Value].Name : "—";

        public static string Names(LoboGame game, IEnumerable<int> indices) => string.Join(", ", indices.Select(i => game.Jugadores[i].Name));

        /// <summary>Quién se despierta en el paso (para que el game master sepa a quién mirar).</summary>
        public static IReadOnlyList<int> Holders(LoboGame game, LoboPaso paso)
        {
            IEnumerable<LoboPlayer> alive = game.Vivos;
            switch (paso)
            {
                case LoboPaso.Enamorados: return alive.Where(p => p.Pareja.HasValue).Select(p => p.Index).ToList();
                case LoboPaso.Lobos: return alive.Where(p => game.EsLobo(p.Index)).Select(p => p.Index).ToList();
                case LoboPaso.Encantados: return alive.Where(p => p.Encantado).Select(p => p.Index).ToList();
                case LoboPaso.Hermanas: return alive.Where(p => p.Rol == LoboRol.Hermana).Select(p => p.Index).ToList();
                case LoboPaso.Hermanos: return alive.Where(p => p.Rol == LoboRol.Hermano).Select(p => p.Index).ToList();
                case LoboPaso.BrujaSalva:
                case LoboPaso.BrujaEnvenena:
                    return alive.Where(p => p.Rol == LoboRol.Bruja).Select(p => p.Index).ToList();
                default:
                {
                    var rol = game.Paso?.Rol;
                    return rol.HasValue ? alive.Where(p => p.Rol == rol.Value).Select(p => p.Index).ToList() : new List<int>();
                }
            }
        }

        /// <summary>El texto de cada opción según el paso.</summary>
        public static string Option(LoboGame game, LoboPaso paso, LoboOpcion opcion)
        {
            switch (opcion)
            {
                case LoboOpcion.Carta1: return Loc.T("loboOpcionCarta", ("n", 1), ("rol", LoboUi.Name(game.CartasLadron[0])));
                case LoboOpcion.Carta2: return Loc.T("loboOpcionCarta", ("n", 2), ("rol", LoboUi.Name(game.CartasLadron[1])));
                case LoboOpcion.Quedarse: return Loc.T("loboOpcionQuedarse");
                case LoboOpcion.Pueblo: return Loc.T("loboOpcionPueblo");
                case LoboOpcion.Lobos: return Loc.T("loboOpcionLobos");
                case LoboOpcion.Si: return Loc.T(paso == LoboPaso.PadreLobos ? "loboOpcionInfectar" : "loboOpcionSalvar");
                default: return Loc.T(paso == LoboPaso.PadreLobos ? "loboOpcionNoInfectar" : "loboOpcionNoSalvar");
            }
        }

        /// <summary>Una línea del registro: «Noche 2 · Los lobos eligen a Ana».</summary>
        public static string Event(LoboGame game, LoboEvento e) =>
            Loc.T("loboEventoLinea", ("cuando", Loc.T(e.Noche ? "loboNocheN" : "loboDiaN", ("n", e.Ronda))), ("texto", EventText(game, e)));

        /// <summary>Lo que dice el evento, sin cuándo pasó.</summary>
        public static string EventText(LoboGame game, LoboEvento e)
        {
            var text = Loc.T("loboEvento" + e.Tipo,
                ("jugador", PlayerName(game, e.Jugador)),
                ("otro", e.Tipo == LoboEventoTipo.Luna ? string.Empty : PlayerName(game, e.Otro)),
                ("rol", e.Rol.HasValue ? LoboUi.Name(e.Rol.Value) : string.Empty),
                ("causa", e.Causa.HasValue ? LoboUi.Cause(e.Causa.Value) : string.Empty),
                ("si", e.Si == true ? Loc.T("si") : Loc.T("no")),
                ("ganador", game.Victoria != null ? Loc.T("loboGanador" + game.Victoria.Ganador) : string.Empty),
                ("luna", e.Tipo == LoboEventoTipo.Luna && e.Otro.HasValue ? Loc.T("loboLuna" + (LoboLuna)e.Otro.Value) : string.Empty));
            if (e.Tipo == LoboEventoTipo.LadronCambia && e.Si == false) text = Loc.T("loboEventoLadronSeQueda", ("jugador", PlayerName(game, e.Jugador)));
            if (e.Tipo == LoboEventoTipo.PerroLoboElige) text = Loc.T(e.Si == true ? "loboEventoPerroLobos" : "loboEventoPerroPueblo", ("jugador", PlayerName(game, e.Jugador)));
            if (e.Tipo == LoboEventoTipo.ZorroOlfatea)
            {
                text = Loc.T(e.Si == true ? "loboEventoZorroSi" : "loboEventoZorroNo", ("jugador", PlayerName(game, e.Jugador)), ("otro", PlayerName(game, e.Otro)));
            }
            if (e.Tipo == LoboEventoTipo.OsoGrune) text = Loc.T(e.Si == true ? "loboEventoOsoSi" : "loboEventoOsoNo");
            if (e.Tipo == LoboEventoTipo.LobosEligen && e.Otro.HasValue)
            {
                text = Loc.T("loboEventoLobosEligenDos", ("jugador", PlayerName(game, e.Jugador)), ("otro", PlayerName(game, e.Otro)));
            }
            if (e.Tipo == LoboEventoTipo.Encanta && !e.Otro.HasValue) text = Loc.T("loboEventoEncantaUno", ("jugador", PlayerName(game, e.Jugador)));
            return text;
        }

        /// <summary>Color de cada final.</summary>
        public static UnityEngine.Color WinnerColor(LoboGanador ganador)
        {
            switch (ganador)
            {
                case LoboGanador.Pueblo: return Palette.NeonCyan;
                case LoboGanador.Lobos: return Palette.NeonRed;
                case LoboGanador.LoboBlanco: return Palette.NeonVioletText;
                case LoboGanador.Enamorados: return Palette.NeonMagenta;
                case LoboGanador.Nadie: return Palette.TextMuted;
                default: return Palette.NeonYellow;
            }
        }
    }
}
