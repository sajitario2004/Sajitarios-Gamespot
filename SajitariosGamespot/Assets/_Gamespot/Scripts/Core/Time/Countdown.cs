using System;

namespace Gamespot.Core
{
    /// <summary>
    /// Cuenta atrás en segundos (V071), sin Unity: la pantalla le pasa el tiempo de cada fotograma con
    /// <see cref="Tick"/> (y no lo hace mientras hay un diálogo abierto). Avisa al cambiar el segundo que se enseña y
    /// al llegar a cero, una sola vez.
    /// </summary>
    public sealed class Countdown
    {
        public event Action<int> SecondChanged;
        public event Action Finished;

        public float Duration { get; private set; }
        public float Remaining { get; private set; }
        public bool Running { get; private set; }
        public bool IsFinished { get; private set; }

        /// <summary>El segundo que se enseña (redondeado hacia arriba: 2,3 s → 3).</summary>
        public int SecondsShown => (int)Math.Ceiling(Remaining);

        /// <summary>Lo que va de 1 (recién empezada) a 0 (acabada).</summary>
        public float Fraction => Duration > 0f ? Remaining / Duration : 0f;

        public void Start(float seconds)
        {
            Duration = Math.Max(0f, seconds);
            Remaining = Duration;
            IsFinished = Duration <= 0f;
            Running = !IsFinished;
            SecondChanged?.Invoke(SecondsShown);
            if (IsFinished) Finished?.Invoke();
        }

        public void Pause() => Running = false;

        public void Resume()
        {
            if (!IsFinished && Remaining > 0f) Running = true;
        }

        public void Stop()
        {
            Running = false;
            Remaining = 0f;
            IsFinished = false;
        }

        /// <summary>Suma tiempo (p. ej. «+30 s» en el debate de Hombres Lobo).</summary>
        public void Add(float seconds)
        {
            if (seconds <= 0f) return;
            Remaining += seconds;
            Duration = Math.Max(Duration, Remaining);
            if (IsFinished)
            {
                IsFinished = false;
                Running = true;
            }
            SecondChanged?.Invoke(SecondsShown);
        }

        public void Tick(float deltaTime)
        {
            if (!Running || deltaTime <= 0f) return;
            var before = SecondsShown;
            Remaining = Math.Max(0f, Remaining - deltaTime);
            if (SecondsShown != before) SecondChanged?.Invoke(SecondsShown);
            if (Remaining > 0f) return;
            Running = false;
            IsFinished = true;
            Finished?.Invoke();
        }
    }
}
