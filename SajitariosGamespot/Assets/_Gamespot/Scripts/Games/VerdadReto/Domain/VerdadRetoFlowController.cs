using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.VerdadReto.Domain
{
    public enum VerdadRetoFase
    {
        Setup,

        /// <summary>Le toca a alguien: elige verdad o reto (o el móvil lo sortea).</summary>
        Eleccion,

        /// <summary>Su carta: la cumple, se raja o pide otra.</summary>
        Carta,

        /// <summary>Se ha rajado: cumple la prenda.</summary>
        Castigo,

        Final,

        /// <summary>No hay cartas para lo elegido.</summary>
        Error
    }

    /// <summary>Marcador de un jugador.</summary>
    public sealed class VerdadRetoPlayer
    {
        public VerdadRetoPlayer(int index, string name)
        {
            Index = index;
            Name = name;
        }

        /// <summary>Posición en el orden de los nombres.</summary>
        public int Index { get; }
        public string Name { get; }
        public int Puntos { get; internal set; }
        public int Verdades { get; internal set; }
        public int Retos { get; internal set; }
        public int Rajadas { get; internal set; }

        /// <summary>Verdades elegidas seguidas (se pone a 0 al elegir reto).</summary>
        public int VerdadesSeguidas { get; internal set; }
    }

    /// <summary>
    /// Partida de Verdad o Reto (V071). Orden de las llamadas al random (forma parte de la especificación):
    /// al empezar, con orden al azar, <c>NextInt(n)</c> elige quién empieza; al sortear el tipo (si los dos se
    /// pueden), <c>NextInt(2)</c> (0 = verdad); cada carta, una llamada del mazo (<see cref="NoRepeatDeck"/>) y, si
    /// lleva <see cref="Otro"/>, <c>NextInt(n − 1)</c> elige al otro jugador; al pasar el turno con orden al azar,
    /// <c>NextInt</c> entre los que no han jugado en la ronda (sin repetir al último si hay más).
    /// </summary>
    public sealed class VerdadRetoFlowController
    {
        /// <summary>Marca de las cartas que nombran a otro jugador al azar («Dale un masaje a {otro}»).</summary>
        public const string Otro = "{otro}";

        private readonly IPromptSource _source;
        private readonly IRandomProvider _random;
        private readonly Dictionary<string, IReadOnlyList<PromptCard>> _pools = new Dictionary<string, IReadOnlyList<PromptCard>>();
        private readonly Dictionary<string, NoRepeatDeck> _decks = new Dictionary<string, NoRepeatDeck>();
        private readonly HashSet<int> _playedThisRound = new HashSet<int>();
        private List<VerdadRetoPlayer> _players = new List<VerdadRetoPlayer>();
        private bool _deckNotice;

        public VerdadRetoFlowController(IPromptSource source, IRandomProvider random)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public event Action Changed;

        public VerdadRetoFase Fase { get; private set; } = VerdadRetoFase.Setup;
        public VerdadRetoConfig Config { get; private set; }
        public IReadOnlyList<VerdadRetoPlayer> Jugadores => _players;

        /// <summary>A quién le toca (índice en <see cref="Jugadores"/>).</summary>
        public int TurnoIndex { get; private set; }

        public VerdadRetoPlayer JugadorActual => _players.Count > 0 ? _players[TurnoIndex] : null;

        /// <summary>Ronda en curso (desde 1).</summary>
        public int Ronda { get; private set; }

        /// <summary>Turnos jugados en total.</summary>
        public int Turnos { get; private set; }

        /// <summary>Lo que eligió (<see cref="VerdadRetoKinds"/>), en la carta y en el castigo.</summary>
        public string TipoActual { get; private set; }

        public PromptCard CartaActual { get; private set; }

        /// <summary>Prenda de quien se raja (nula si no hay cartas de castigo: la elige el grupo).</summary>
        public PromptCard CastigoActual { get; private set; }

        /// <summary>El otro jugador que nombra la carta (o el castigo), si lleva <see cref="Otro"/>.</summary>
        public string OtroJugador { get; private set; }

        public int CambiosRestantes { get; private set; }

        /// <summary>Texto de la carta en curso con el nombre del otro jugador ya puesto.</summary>
        public string TextoCarta => Render(CartaActual);

        public string TextoCastigo => Render(CastigoActual);

        /// <summary>Si hay cartas de ese tipo para los niveles elegidos.</summary>
        public bool Disponible(string kind) => _pools.TryGetValue(kind, out var pool) && pool.Count > 0;

        /// <summary>
        /// Si el jugador de turno no puede elegir verdad porque ya lleva el máximo de verdades seguidas (solo si hay
        /// retos que elegir).
        /// </summary>
        public bool VerdadBloqueada =>
            Config != null && Config.Options.MaxVerdadesSeguidas > 0 && Disponible(VerdadRetoKinds.Reto) && JugadorActual != null
            && JugadorActual.VerdadesSeguidas >= Config.Options.MaxVerdadesSeguidas;

        public bool PuedeElegir(string kind)
        {
            if (Fase != VerdadRetoFase.Eleccion || !Disponible(kind)) return false;
            if (kind == VerdadRetoKinds.Verdad) return !VerdadBloqueada;
            return kind == VerdadRetoKinds.Reto;
        }

        public bool PuedeRajarse => Fase == VerdadRetoFase.Carta && Config.Options.PermitirRajarse;

        /// <summary>Si se está jugando la última ronda (sin límite de rondas, nunca).</summary>
        public bool UltimaRonda => Config != null && Config.Options.Rondas > 0 && Ronda >= Config.Options.Rondas;

        /// <summary>Clasificación: más puntos, menos rajadas y, a igualdad, el orden de los nombres.</summary>
        public IReadOnlyList<VerdadRetoPlayer> Ranking =>
            _players.OrderByDescending(p => p.Puntos).ThenBy(p => p.Rajadas).ThenBy(p => p.Index).ToList();

        public void Iniciar(VerdadRetoConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _players = config.Jugadores.Select((n, i) => new VerdadRetoPlayer(i, n)).ToList();
            _pools.Clear();
            _decks.Clear();
            _playedThisRound.Clear();
            CartaActual = null;
            CastigoActual = null;
            OtroJugador = null;
            TipoActual = null;
            Turnos = 0;
            Ronda = 1;
            _deckNotice = false;
            try
            {
                foreach (var kind in VerdadRetoKinds.All)
                {
                    _pools[kind] = _source.GetCards(new PromptQuery
                    {
                        Levels = config.Niveles,
                        Kind = kind,
                        IncludeSeed = config.Options.IncluirSerie,
                        IncludeOwn = config.Options.IncluirPropias
                    }) ?? Array.Empty<PromptCard>();
                    _decks[kind] = new NoRepeatDeck(_random);
                }
            }
            catch (Exception)
            {
                Fail();
                return;
            }
            if (!Disponible(VerdadRetoKinds.Verdad) && !Disponible(VerdadRetoKinds.Reto))
            {
                Fail();
                return;
            }
            TurnoIndex = config.Options.Orden == OrdenTurnos.AlAzar ? _random.NextInt(_players.Count) : 0;
            _playedThisRound.Add(TurnoIndex);
            Fase = VerdadRetoFase.Eleccion;
            Changed?.Invoke();
        }

        /// <summary>El jugador elige verdad o reto y sale su carta.</summary>
        public void Elegir(string kind)
        {
            if (!PuedeElegir(kind)) return;
            var player = JugadorActual;
            player.VerdadesSeguidas = kind == VerdadRetoKinds.Verdad ? player.VerdadesSeguidas + 1 : 0;
            TipoActual = kind;
            CambiosRestantes = Config.Options.CambiosPorTurno;
            CastigoActual = null;
            CartaActual = Draw(kind);
            Fase = VerdadRetoFase.Carta;
            Changed?.Invoke();
        }

        /// <summary>El móvil decide verdad o reto entre los que se pueden elegir.</summary>
        public void Sortear()
        {
            if (Fase != VerdadRetoFase.Eleccion) return;
            var verdad = PuedeElegir(VerdadRetoKinds.Verdad);
            var reto = PuedeElegir(VerdadRetoKinds.Reto);
            if (!verdad && !reto) return;
            var kind = verdad && reto
                ? (_random.NextInt(2) == 0 ? VerdadRetoKinds.Verdad : VerdadRetoKinds.Reto)
                : verdad ? VerdadRetoKinds.Verdad : VerdadRetoKinds.Reto;
            Elegir(kind);
        }

        /// <summary>Otra carta del mismo tipo (si quedan cambios en el turno).</summary>
        public void OtraCarta()
        {
            if (Fase != VerdadRetoFase.Carta || CambiosRestantes <= 0) return;
            CambiosRestantes--;
            CartaActual = Draw(TipoActual);
            Changed?.Invoke();
        }

        /// <summary>Hecho: suma sus puntos y pasa el turno.</summary>
        public void Cumplir()
        {
            if (Fase != VerdadRetoFase.Carta) return;
            var player = JugadorActual;
            if (TipoActual == VerdadRetoKinds.Verdad) player.Verdades++;
            else player.Retos++;
            if (Config.Options.Puntos) player.Puntos += TipoActual == VerdadRetoKinds.Verdad ? VerdadRetoConfig.PuntosVerdad : VerdadRetoConfig.PuntosReto;
            Avanzar();
        }

        /// <summary>«Me rajo»: no suma y sale una prenda.</summary>
        public void Rajarse()
        {
            if (!PuedeRajarse) return;
            JugadorActual.Rajadas++;
            OtroJugador = null;
            CastigoActual = Disponible(VerdadRetoKinds.Castigo) ? Draw(VerdadRetoKinds.Castigo) : null;
            Fase = VerdadRetoFase.Castigo;
            Changed?.Invoke();
        }

        /// <summary>Prenda cumplida: pasa el turno.</summary>
        public void CastigoCumplido()
        {
            if (Fase != VerdadRetoFase.Castigo) return;
            Avanzar();
        }

        /// <summary>Acaba la partida ya (botón «Terminar»).</summary>
        public void Terminar()
        {
            if (Fase == VerdadRetoFase.Setup || Fase == VerdadRetoFase.Error || Fase == VerdadRetoFase.Final) return;
            Fase = VerdadRetoFase.Final;
            Changed?.Invoke();
        }

        public void Reiniciar()
        {
            Fase = VerdadRetoFase.Setup;
            Config = null;
            _players = new List<VerdadRetoPlayer>();
            _pools.Clear();
            _decks.Clear();
            _playedThisRound.Clear();
            CartaActual = null;
            CastigoActual = null;
            OtroJugador = null;
            TipoActual = null;
            Changed?.Invoke();
        }

        /// <summary>Si en la última carta se acabó el mazo de su tipo y volvió a empezar (para avisar una vez).</summary>
        public bool ConsumirAvisoMazo()
        {
            var notice = _deckNotice;
            _deckNotice = false;
            return notice;
        }

        private PromptCard Draw(string kind)
        {
            var deck = _decks[kind];
            var card = deck.Draw(_pools[kind]);
            if (deck.Reshuffled) _deckNotice = true;
            OtroJugador = card.Text.Contains(Otro) || (card.TextB != null && card.TextB.Contains(Otro)) ? PickOther() : null;
            return card;
        }

        private string PickOther()
        {
            var others = _players.Where(p => p.Index != TurnoIndex).ToList();
            return others.Count == 0 ? null : others[_random.NextInt(others.Count)].Name;
        }

        private string Render(PromptCard card)
        {
            if (card == null) return null;
            return OtroJugador == null ? card.Text : card.Text.Replace(Otro, OtroJugador);
        }

        private void Avanzar()
        {
            Turnos++;
            CartaActual = null;
            CastigoActual = null;
            OtroJugador = null;
            var n = _players.Count;
            if (_playedThisRound.Count >= n)
            {
                if (UltimaRonda)
                {
                    Fase = VerdadRetoFase.Final;
                    Changed?.Invoke();
                    return;
                }
                Ronda++;
                _playedThisRound.Clear();
            }
            var last = TurnoIndex;
            if (Config.Options.Orden == OrdenTurnos.EnOrden)
            {
                TurnoIndex = (last + 1) % n;
            }
            else
            {
                var candidates = Enumerable.Range(0, n).Where(i => !_playedThisRound.Contains(i)).ToList();
                if (candidates.Count > 1) candidates.Remove(last);
                TurnoIndex = candidates[_random.NextInt(candidates.Count)];
            }
            _playedThisRound.Add(TurnoIndex);
            Fase = VerdadRetoFase.Eleccion;
            Changed?.Invoke();
        }

        private void Fail()
        {
            Fase = VerdadRetoFase.Error;
            Changed?.Invoke();
        }
    }
}
