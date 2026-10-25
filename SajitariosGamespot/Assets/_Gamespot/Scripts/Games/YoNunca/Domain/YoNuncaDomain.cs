using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.YoNunca.Domain
{
    /// <summary>Nivel de las frases. Se guarda como <c>suave</c> / <c>picante</c>.</summary>
    public enum Intensidad
    {
        Suave,
        Picante
    }

    public static class Intensidades
    {
        public static string StorageName(this Intensidad i) => i == Intensidad.Suave ? "suave" : "picante";

        public static Intensidad FromStorageName(string name)
        {
            switch (name)
            {
                case "suave": return Intensidad.Suave;
                case "picante": return Intensidad.Picante;
                default: throw new FormatException($"Intensidad desconocida: {name}");
            }
        }
    }

    /// <summary>Frase "Yo nunca…" con su nivel.</summary>
    public sealed class NeverStatement : IEquatable<NeverStatement>
    {
        public NeverStatement(long id, string frase, Intensidad intensidad)
        {
            Frase = frase?.Trim() ?? string.Empty;
            if (Frase.Length == 0) throw new ArgumentException("La frase no puede estar vacía", nameof(frase));
            Id = id;
            Intensidad = intensidad;
        }

        public long Id { get; }
        public string Frase { get; }
        public Intensidad Intensidad { get; }

        public bool Equals(NeverStatement other) => other != null && other.Id == Id && other.Frase == Frase && other.Intensidad == Intensidad;
        public override bool Equals(object obj) => Equals(obj as NeverStatement);
        public override int GetHashCode() => (Id.GetHashCode() * 397) ^ Frase.GetHashCode();
    }

    /// <summary>Niveles elegidos (al menos uno).</summary>
    public sealed class YoNuncaConfig
    {
        private YoNuncaConfig(IReadOnlyCollection<Intensidad> intensidades) => Intensidades = intensidades;

        public IReadOnlyCollection<Intensidad> Intensidades { get; }

        /// <summary>Nulo si no hay ninguna intensidad (error <c>sinIntensidades</c>).</summary>
        public static YoNuncaConfig Create(IEnumerable<Intensidad> intensidades)
        {
            var set = new HashSet<Intensidad>(intensidades ?? Array.Empty<Intensidad>());
            return set.Count == 0 ? null : new YoNuncaConfig(set);
        }
    }

    /// <summary>
    /// Saca una frase de los niveles elegidos sin repetir hasta agotarlas; al agotarse se vacían las vistas y se
    /// vuelve a empezar (la última puede repetirse justo después). <paramref name="seen"/> se modifica.
    /// </summary>
    public sealed class DrawStatementUseCase
    {
        private readonly IRandomProvider _random;

        public DrawStatementUseCase(IRandomProvider random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        public NeverStatement Execute(IReadOnlyList<NeverStatement> pool, IReadOnlyCollection<Intensidad> intensidades, HashSet<long> seen)
        {
            var filtered = (pool ?? Array.Empty<NeverStatement>()).Where(s => intensidades.Contains(s.Intensidad)).ToList();
            if (filtered.Count == 0) throw new ArgumentException("No hay frases para esas intensidades");
            var remaining = filtered.Where(s => !seen.Contains(s.Id)).ToList();
            if (remaining.Count == 0)
            {
                seen.Clear();
                remaining = filtered;
            }
            var chosen = remaining[_random.NextInt(remaining.Count)];
            seen.Add(chosen.Id);
            return chosen;
        }
    }

    public enum YoNuncaFase
    {
        Setup,
        Jugando,
        Error
    }

    /// <summary>De dónde salen las frases (el repositorio, en la app).</summary>
    public interface INeverStatementSource
    {
        IReadOnlyList<NeverStatement> GetByIntensidades(IReadOnlyCollection<Intensidad> intensidades);
    }

    /// <summary>Partida infinita de Yo Nunca: una frase tras otra del nivel elegido.</summary>
    public sealed class YoNuncaFlowController
    {
        private readonly INeverStatementSource _source;
        private readonly DrawStatementUseCase _draw;
        private readonly HashSet<long> _seen = new HashSet<long>();
        private IReadOnlyList<NeverStatement> _pool = Array.Empty<NeverStatement>();

        public YoNuncaFlowController(INeverStatementSource source, IRandomProvider random)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _draw = new DrawStatementUseCase(random);
        }

        public event Action Changed;

        public YoNuncaFase Fase { get; private set; } = YoNuncaFase.Setup;
        public YoNuncaConfig Config { get; private set; }
        public NeverStatement Current { get; private set; }

        public void Iniciar(YoNuncaConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _seen.Clear();
            try
            {
                _pool = _source.GetByIntensidades(config.Intensidades) ?? Array.Empty<NeverStatement>();
                if (_pool.Count == 0)
                {
                    Fail();
                    return;
                }
                Current = _draw.Execute(_pool, config.Intensidades, _seen);
                Fase = YoNuncaFase.Jugando;
            }
            catch (Exception)
            {
                Fail();
                return;
            }
            Changed?.Invoke();
        }

        public void Siguiente()
        {
            if (Fase != YoNuncaFase.Jugando) return;
            Current = _draw.Execute(_pool, Config.Intensidades, _seen);
            Changed?.Invoke();
        }

        public void Reiniciar()
        {
            Fase = YoNuncaFase.Setup;
            Config = null;
            Current = null;
            _seen.Clear();
            _pool = Array.Empty<NeverStatement>();
            Changed?.Invoke();
        }

        private void Fail()
        {
            Fase = YoNuncaFase.Error;
            Current = null;
            Changed?.Invoke();
        }
    }
}
