using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Bomba.Domain
{
    public enum BombaFase
    {
        Setup,
        Iniciando,
        Jugando,
        Explotando,
        GameOver,
        Error
    }

    public enum BombaErrorKind
    {
        SinPrompts
    }

    /// <summary>De dónde salen las sílabas y las categorías (el repositorio, en la app).</summary>
    public interface IBombaPromptSource
    {
        IReadOnlyList<BombaPrompt> GetAllSilabas();
        IReadOnlyList<BombaPrompt> GetAllCategorias();
    }

    /// <summary>
    /// Partida de La Bomba (port de <c>BombaFlowController</c>). La mecha se cuenta en la pantalla; este
    /// controlador es puro: pasar no reinicia la mecha y, tras la explosión, la siguiente ronda trae mecha y
    /// sílaba o categoría nuevas.
    /// </summary>
    public sealed class BombaFlowController
    {
        private readonly IBombaPromptSource _prompts;
        private readonly IRandomProvider _random;
        private PickPromptUseCase _pick;
        private IReadOnlyList<BombaPrompt> _pool = Array.Empty<BombaPrompt>();

        public BombaFlowController(IBombaPromptSource prompts, IRandomProvider random)
        {
            _prompts = prompts ?? throw new ArgumentNullException(nameof(prompts));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public event Action Changed;

        public BombaFase Fase { get; private set; } = BombaFase.Setup;
        public BombaConfig Config { get; private set; }
        public BombaSession Session { get; private set; }
        public BombaPrompt CurrentPrompt { get; private set; }

        /// <summary>Quién acaba de explotar (solo en <see cref="BombaFase.Explotando"/>).</summary>
        public string Eliminado { get; private set; }

        public BombaErrorKind? ErrorKind { get; private set; }

        public double FuseSeconds => Session?.FuseSeconds ?? 0d;
        public string CurrentHolder => Session?.CurrentHolder;
        public IReadOnlyList<string> AlivePlayers => Session?.AlivePlayers ?? (IReadOnlyList<string>)Array.Empty<string>();
        public string Winner => Session?.Winner;

        public void Iniciar(BombaConfig config)
        {
            Reset();
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Fase = BombaFase.Iniciando;
            Notify();
            try
            {
                _pool = (_prompts.GetAllSilabas() ?? Array.Empty<BombaPrompt>()).Concat(_prompts.GetAllCategorias() ?? Array.Empty<BombaPrompt>()).ToList();
                if (!_pool.Any(p => p.Mode == config.Mode))
                {
                    Fail();
                    return;
                }
                _pick = new PickPromptUseCase(_random);
                Session = BombaSession.Start(config, _random);
                CurrentPrompt = _pick.Pick(config.Mode, _pool);
                Fase = BombaFase.Jugando;
            }
            catch (Exception)
            {
                Fail();
                return;
            }
            Notify();
        }

        /// <summary>La bomba pasa al siguiente jugador vivo. La mecha sigue corriendo.</summary>
        public void Pasar()
        {
            if (Fase != BombaFase.Jugando || Session == null || Session.IsOver) return;
            Session = Session.Pasar();
            Notify();
        }

        /// <summary>Se acabó la mecha: queda eliminado quien tiene la bomba.</summary>
        public void Explotar()
        {
            if (Fase != BombaFase.Jugando || Session == null || Session.IsOver) return;
            Eliminado = Session.CurrentHolder;
            Session = Session.Explode();
            Fase = BombaFase.Explotando;
            Notify();
        }

        /// <summary>Tras el aviso de la explosión: fin si queda uno, si no ronda nueva con mecha y prompt nuevos.</summary>
        public void ContinuarTrasExplosion()
        {
            if (Fase != BombaFase.Explotando || Session == null) return;
            Eliminado = null;
            if (Session.IsOver)
            {
                Fase = BombaFase.GameOver;
                Notify();
                return;
            }
            Session = BombaSession.NewRound(Session, Config, _random);
            CurrentPrompt = _pick?.Pick(Config.Mode, _pool) ?? CurrentPrompt;
            Fase = BombaFase.Jugando;
            Notify();
        }

        public void Reiniciar()
        {
            Reset();
            Notify();
        }

        private void Fail()
        {
            Fase = BombaFase.Error;
            Session = null;
            CurrentPrompt = null;
            ErrorKind = BombaErrorKind.SinPrompts;
            Notify();
        }

        private void Reset()
        {
            Fase = BombaFase.Setup;
            Config = null;
            Session = null;
            CurrentPrompt = null;
            Eliminado = null;
            ErrorKind = null;
            _pick = null;
            _pool = Array.Empty<BombaPrompt>();
        }

        private void Notify() => Changed?.Invoke();
    }
}
