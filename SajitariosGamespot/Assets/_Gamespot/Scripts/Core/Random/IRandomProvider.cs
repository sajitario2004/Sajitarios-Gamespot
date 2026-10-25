namespace Gamespot.Core
{
    /// <summary>
    /// Fuente de aleatoriedad inyectable (port de <c>RandomProvider</c> del Flutter).
    /// El dominio nunca usa <c>System.Random</c> ni <c>UnityEngine.Random</c> directamente:
    /// recibe esta interfaz para poder fijar la secuencia en los tests.
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>Entero en [0, max). Lanza si <paramref name="max"/> es menor o igual que 0.</summary>
        int NextInt(int max);

        /// <summary>Real en [0, 1).</summary>
        double NextDouble();

        /// <summary>Verdadero o falso con la misma probabilidad.</summary>
        bool NextBool();
    }
}
