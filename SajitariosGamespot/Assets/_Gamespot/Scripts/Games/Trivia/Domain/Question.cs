using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Games.Trivia.Domain
{
    /// <summary>Tramos de dificultad. En la BD y el seed se guarda el nombre: <c>facil</c>, <c>dificil</c>, <c>muyDificil</c>.</summary>
    public enum Difficulty
    {
        Facil,
        Dificil,
        MuyDificil
    }

    public static class Difficulties
    {
        public static readonly IReadOnlyList<Difficulty> All = new[] { Difficulty.Facil, Difficulty.Dificil, Difficulty.MuyDificil };

        /// <summary>Nombre guardado en la BD y el seed.</summary>
        public static string StorageName(this Difficulty d) => d == Difficulty.Facil ? "facil" : d == Difficulty.Dificil ? "dificil" : "muyDificil";

        public static Difficulty FromStorageName(string name)
        {
            switch (name)
            {
                case "facil": return Difficulty.Facil;
                case "dificil": return Difficulty.Dificil;
                case "muyDificil": return Difficulty.MuyDificil;
                default: throw new ArgumentException($"Dificultad desconocida: {name}", nameof(name));
            }
        }

        /// <summary>OpenTDB easy/medium/hard → fácil/difícil/muy difícil.</summary>
        public static Difficulty FromOpenTdb(string value)
        {
            switch (value)
            {
                case "easy": return Difficulty.Facil;
                case "medium": return Difficulty.Dificil;
                case "hard": return Difficulty.MuyDificil;
                default: throw new ArgumentException($"Dificultad de OpenTDB desconocida: {value}", nameof(value));
            }
        }
    }

    /// <summary>Temáticas de la trivia (el nombre visible lo pone la presentación, localizado).</summary>
    public static class Tematicas
    {
        public static readonly IReadOnlyList<string> All = new[]
        {
            "cultura_general", "videojuegos", "cocina", "cine", "ciencia", "geografia", "historia", "deportes", "musica"
        };

        /// <summary>Marcadas por defecto en el setup.</summary>
        public static readonly IReadOnlyList<string> Default = new[] { "cultura_general", "historia" };
    }

    /// <summary>Pregunta con exactamente 4 opciones y el índice de la correcta en [0, 4).</summary>
    public sealed class Question : IEquatable<Question>
    {
        public const int OptionCount = 4;

        public Question(long id, string tematicaId, Difficulty difficulty, string enunciado, IReadOnlyList<string> options, int correctIndex)
        {
            if (options == null || options.Count != OptionCount) throw new ArgumentException("Una pregunta tiene exactamente 4 opciones", nameof(options));
            if (correctIndex < 0 || correctIndex >= OptionCount) throw new ArgumentOutOfRangeException(nameof(correctIndex), correctIndex, "Índice fuera de [0, 4)");
            if (string.IsNullOrWhiteSpace(enunciado)) throw new ArgumentException("El enunciado no puede estar vacío", nameof(enunciado));
            Id = id;
            TematicaId = tematicaId;
            Difficulty = difficulty;
            Enunciado = enunciado;
            Options = options.ToList().AsReadOnly();
            CorrectIndex = correctIndex;
        }

        public long Id { get; }
        public string TematicaId { get; }
        public Difficulty Difficulty { get; }
        public string Enunciado { get; }
        public IReadOnlyList<string> Options { get; }
        public int CorrectIndex { get; }

        public bool IsCorrect(int index) => index == CorrectIndex;

        public bool Equals(Question other) => other != null && other.Id == Id && other.Enunciado == Enunciado;
        public override bool Equals(object obj) => Equals(obj as Question);
        public override int GetHashCode() => (Id.GetHashCode() * 397) ^ Enunciado.GetHashCode();
    }
}
