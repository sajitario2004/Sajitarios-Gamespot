using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Games.Impostor.Domain;

namespace Gamespot.Games.Impostor.Data
{
    /// <summary>
    /// Fuente de palabras de la partida limitada a las categorías que se marcaron en el setup (V059i). Sin
    /// categorías marcadas (nulo), todas. El dominio sigue recibiendo una lista de palabras: no sabe de categorías.
    /// </summary>
    public sealed class CategoryWordSource : IImpostorWordSource
    {
        private readonly WordRepository _words;

        public CategoryWordSource(WordRepository words) => _words = words;

        public IReadOnlyCollection<string> Categories { get; set; }

        public IReadOnlyList<Word> GetAllWords() => Categories == null ? _words.GetAllWords() : _words.GetWords(Categories);
    }
}
