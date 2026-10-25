using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Tabu.Domain
{
    public enum TabuFase
    {
        Setup,
        Iniciando,
        Turno,
        FinRonda,
        GameOver,
        Error
    }

    public enum TabuErrorKind
    {
        SinPalabras
    }

    /// <summary>De dónde salen las palabras (el repositorio, en la app).</summary>
    public interface ITabuWordSource
    {
        IReadOnlyList<TabuWord> GetAll();
    }

    /// <summary>
    /// Partida de Tabú (port de <c>TabuFlowController</c>). El temporizador del turno vive en la pantalla; este
    /// controlador es puro. Cada acierto, salto o falta pasa a una palabra que no haya salido (si se acaban, se
    /// queda la actual).
    /// </summary>
    public sealed class TabuFlowController
    {
        private readonly ITabuWordSource _words;
        private readonly PickTabuWordUseCase _pick;
        private IReadOnlyList<TabuWord> _pool = Array.Empty<TabuWord>();

        public TabuFlowController(ITabuWordSource words, IRandomProvider random)
        {
            _words = words ?? throw new ArgumentNullException(nameof(words));
            _pick = new PickTabuWordUseCase(random);
        }

        public event Action Changed;

        public TabuFase Fase { get; private set; } = TabuFase.Setup;
        public TabuConfig Config { get; private set; }
        public TabuSession Session { get; private set; }
        public TabuErrorKind? ErrorKind { get; private set; }

        public int TurnDurationSeconds => Config?.TurnoSegundos ?? TabuConfig.DefaultTurnSeconds;
        public string CurrentTeamName => Config != null && Session != null ? Config.NameOf(Session.EquipoActual) : null;

        public void Iniciar(TabuConfig config)
        {
            Reset();
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Fase = TabuFase.Iniciando;
            Notify();
            try
            {
                _pool = _words.GetAll() ?? Array.Empty<TabuWord>();
                if (_pool.Count == 0)
                {
                    Fail();
                    return;
                }
                Session = TabuSession.Start(config, _pick.Execute(_pool, Array.Empty<long>()));
                Fase = TabuFase.Turno;
            }
            catch (Exception)
            {
                Fail();
                return;
            }
            Notify();
        }

        public void Acierto() => Act(s => s.RegistrarAcierto());

        public void Saltar() => Act(s => s.RegistrarSalto());

        public void Falta() => Act(s => s.RegistrarFalta());

        /// <summary>Se acabó el tiempo del turno.</summary>
        public void TerminarTurno()
        {
            if (Fase != TabuFase.Turno || Session == null || Session.IsOver) return;
            var next = PickNext(Session) ?? Session.PalabraActual;
            Session = Session.TerminarTurno(next);
            Fase = Session.IsOver ? TabuFase.GameOver : TabuFase.FinRonda;
            Notify();
        }

        /// <summary>Del marcador al turno del otro equipo.</summary>
        public void SiguienteTurno()
        {
            if (Fase != TabuFase.FinRonda) return;
            Fase = TabuFase.Turno;
            Notify();
        }

        public void Reiniciar()
        {
            Reset();
            Notify();
        }

        private void Act(Func<TabuSession, TabuSession> change)
        {
            if (Fase != TabuFase.Turno || Session == null || Session.IsOver) return;
            var updated = change(Session);
            var next = PickNext(updated);
            Session = next != null ? updated.AvanzarPalabra(next) : updated;
            Notify();
        }

        private TabuWord PickNext(TabuSession session)
        {
            var used = session.PalabrasUsadas;
            var available = _pool.Where(w => !used.Contains(w.Id)).ToList();
            return available.Count == 0 ? null : _pick.Execute(available, Array.Empty<long>());
        }

        private void Fail()
        {
            Fase = TabuFase.Error;
            Session = null;
            ErrorKind = TabuErrorKind.SinPalabras;
            Notify();
        }

        private void Reset()
        {
            Fase = TabuFase.Setup;
            Config = null;
            Session = null;
            ErrorKind = null;
            _pool = Array.Empty<TabuWord>();
        }

        private void Notify() => Changed?.Invoke();
    }
}
