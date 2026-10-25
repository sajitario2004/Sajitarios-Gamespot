using System;
using System.Collections.Generic;

namespace Gamespot.Core
{
    /// <summary>Operaciones comunes sobre <see cref="IRandomProvider"/>, con el mismo orden de llamadas que el Flutter.</summary>
    public static class RandomExtensions
    {
        /// <summary>Un elemento al azar: <c>items[NextInt(count)]</c>. Lanza con la lista vacía.</summary>
        public static T Pick<T>(this IRandomProvider rng, IReadOnlyList<T> items)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            if (items == null || items.Count == 0) throw new ArgumentException("No se puede elegir de una lista vacía", nameof(items));
            return items[rng.NextInt(items.Count)];
        }

        /// <summary>
        /// Baraja en el sitio con Fisher-Yates, exactamente como el Flutter:
        /// <c>for i = n-1 .. 1: j = NextInt(i + 1); swap(i, j)</c>.
        /// </summary>
        public static void Shuffle<T>(this IRandomProvider rng, IList<T> list)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            if (list == null) throw new ArgumentNullException(nameof(list));
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.NextInt(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>Copia barajada; la lista original no se toca.</summary>
        public static List<T> Shuffled<T>(this IRandomProvider rng, IEnumerable<T> items)
        {
            var copy = new List<T>(items);
            rng.Shuffle(copy);
            return copy;
        }
    }
}
