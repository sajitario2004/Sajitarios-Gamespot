using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Wavelength.Domain
{
    public enum WavelengthFase
    {
        Setup,
        Iniciando,
        Clue,
        Pass,
        Guess,
        Reveal,
        GameOver,
        Error
    }

    public enum WavelengthErrorKind
    {
        SinEspectros
    }

    /// <summary>De dónde salen los espectros (el repositorio, en la app).</summary>
    public interface ISpectrumSource
    {
        IReadOnlyList<Spectrum> GetAll();
    }

    /// <summary>
    /// Partida de Wavelength (port de <c>WavelengthFlowController</c>): pista del psíquico → pasar el móvil →
    /// el grupo mueve el dial → revelado → siguiente ronda o fin. La ronda se suma al marcador al pasar a la
    /// siguiente (<see cref="Next"/>). Cada acción en una fase que no le toca no hace nada.
    /// </summary>
    public sealed class WavelengthFlowController
    {
        private readonly ISpectrumSource _spectra;
        private readonly PickRoundUseCase _pick;

        public WavelengthFlowController(ISpectrumSource spectra, IRandomProvider random)
        {
            _spectra = spectra ?? throw new ArgumentNullException(nameof(spectra));
            _pick = new PickRoundUseCase(random);
        }

        public event Action Changed;

        public WavelengthFase Fase { get; private set; } = WavelengthFase.Setup;
        public WavelengthConfig Config { get; private set; }
        public WavelengthSession Session { get; private set; }
        public WavelengthRound CurrentRound { get; private set; }
        public WavelengthErrorKind? ErrorKind { get; private set; }

        public string CurrentPsychic => Session?.CurrentPsychic;

        /// <summary>Marcador incluyendo la ronda que se está revelando (lo que se enseña en el revelado).</summary>
        public int ScoreIncludingCurrent => (Session?.CumulativeScore ?? 0) + (Fase == WavelengthFase.Reveal ? CurrentRound?.Score ?? 0 : 0);

        public void Iniciar(WavelengthConfig config)
        {
            Reset();
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Fase = WavelengthFase.Iniciando;
            Notify();
            try
            {
                var pool = _spectra.GetAll();
                if (pool == null || pool.Count == 0)
                {
                    Fail();
                    return;
                }
                Session = WavelengthSession.Start(config.PlayerNames, config.Rounds);
                CurrentRound = _pick.Execute(pool);
                Fase = WavelengthFase.Clue;
            }
            catch (Exception)
            {
                Fail();
                return;
            }
            Notify();
        }

        /// <summary>El psíquico confirma su pista (vacía no hace nada).</summary>
        public void ConfirmarPista(string pista)
        {
            if (Fase != WavelengthFase.Clue || CurrentRound == null) return;
            var trimmed = pista?.Trim() ?? string.Empty;
            if (trimmed.Length == 0) return;
            CurrentRound = CurrentRound.WithClue(trimmed);
            Fase = WavelengthFase.Pass;
            Notify();
        }

        public void PasarDispositivo()
        {
            if (Fase != WavelengthFase.Pass) return;
            Fase = WavelengthFase.Guess;
            Notify();
        }

        /// <summary>El grupo fija el dial en <paramref name="position"/> (se recorta a [0, 1]).</summary>
        public void SubmitGuess(double position)
        {
            if (Fase != WavelengthFase.Guess || CurrentRound == null) return;
            CurrentRound = CurrentRound.WithGuess(position);
            Fase = WavelengthFase.Reveal;
            Notify();
        }

        /// <summary>Suma la ronda y pasa a la siguiente o al fin. Si falla la carga de espectros, termina la partida.</summary>
        public void Next()
        {
            if (Fase != WavelengthFase.Reveal || Session == null || CurrentRound == null || !CurrentRound.HasGuess) return;
            var updated = Session.RecordRound(CurrentRound);
            Session = updated;
            if (updated.IsOver)
            {
                CurrentRound = null;
                Fase = WavelengthFase.GameOver;
                Notify();
                return;
            }
            try
            {
                CurrentRound = _pick.Execute(Unused(_spectra.GetAll()));
                Fase = WavelengthFase.Clue;
            }
            catch (Exception)
            {
                CurrentRound = null;
                Fase = WavelengthFase.GameOver;
            }
            Notify();
        }

        /// <summary>
        /// Espectros que aún no han salido en la partida (W2; en el Flutter podían repetirse). Si ya salieron
        /// todos, vuelven a estar todos.
        /// </summary>
        private IReadOnlyList<Spectrum> Unused(IReadOnlyList<Spectrum> pool)
        {
            var used = new HashSet<long>(Session.CompletedRounds.Select(r => r.Spectrum.Id));
            var fresh = (pool ?? Array.Empty<Spectrum>()).Where(s => !used.Contains(s.Id)).ToList();
            return fresh.Count > 0 ? fresh : pool;
        }

        public void Reiniciar()
        {
            Reset();
            Notify();
        }

        private void Fail()
        {
            Fase = WavelengthFase.Error;
            Session = null;
            CurrentRound = null;
            ErrorKind = WavelengthErrorKind.SinEspectros;
            Notify();
        }

        private void Reset()
        {
            Fase = WavelengthFase.Setup;
            Config = null;
            Session = null;
            CurrentRound = null;
            ErrorKind = null;
        }

        private void Notify() => Changed?.Invoke();
    }
}
