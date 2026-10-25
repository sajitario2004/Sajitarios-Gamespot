using System;
using System.Collections.Generic;

namespace Gamespot.Core
{
    /// <summary>
    /// <see cref="IRandomProvider"/> guionizado para tests: devuelve, en orden, los reales de la lista.
    /// <c>NextInt(max)</c> usa el siguiente real como <c>(int)(d * max)</c>, así un mismo guion sirve
    /// para enteros y reales. Lanza si se acaba el guion (salvo que se pida repetirlo).
    /// </summary>
    public sealed class ScriptedRandom : IRandomProvider
    {
        private readonly IReadOnlyList<double> _values;
        private readonly bool _loop;
        private int _index;

        public ScriptedRandom(params double[] values) : this(false, values)
        {
        }

        public ScriptedRandom(bool loop, params double[] values)
        {
            if (values == null || values.Length == 0) throw new ArgumentException("El guion no puede estar vacío", nameof(values));
            foreach (var v in values)
            {
                if (v < 0 || v >= 1) throw new ArgumentOutOfRangeException(nameof(values), v, "Cada valor tiene que estar en [0, 1)");
            }
            _values = values;
            _loop = loop;
        }

        /// <summary>Cuántos valores del guion se han consumido.</summary>
        public int Consumed => _index;

        public int NextInt(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max), max, "max tiene que ser mayor que 0");
            return Math.Min(max - 1, (int)(Next() * max));
        }

        public double NextDouble() => Next();

        public bool NextBool() => Next() >= 0.5;

        private double Next()
        {
            if (_index >= _values.Count)
            {
                if (!_loop) throw new InvalidOperationException($"El guion de ScriptedRandom se ha acabado tras {_values.Count} valores");
                _index = 0;
            }
            return _values[_index++];
        }
    }
}
