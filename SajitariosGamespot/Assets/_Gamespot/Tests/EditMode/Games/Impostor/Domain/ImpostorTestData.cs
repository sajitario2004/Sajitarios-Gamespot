using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Games.Impostor.Domain;

namespace Gamespot.Tests.EditMode.Games.Impostor
{
    /// <summary>Datos y dobles de prueba compartidos por los tests del Impostor.</summary>
    internal static class ImpostorTestData
    {
        public static List<Player> Players(params string[] names) => names.Select(n => new Player(n)).ToList();

        public static List<Player> Players(int count) => Enumerable.Range(1, count).Select(i => new Player($"J{i}")).ToList();

        public static GameConfig Config(IReadOnlyList<Player> players, int nImpostores, bool hint = false, int? rounds = null)
        {
            var result = GameConfig.Create(players, nImpostores, hint, rounds);
            if (!result.IsSuccess) throw new InvalidOperationException($"Config inválida en el test: {result.Error}");
            return result.Config;
        }

        public static readonly IReadOnlyList<Word> Words = new[]
        {
            new Word("playa", "arena"),
            new Word("pirata", "barco"),
            new Word("montaña", "nieve")
        };
    }

    /// <summary>Fuente de palabras fija (o que falla) para el flow controller.</summary>
    internal sealed class FakeWordSource : IImpostorWordSource
    {
        private readonly IReadOnlyList<Word> _words;
        private readonly bool _throws;

        public FakeWordSource(IReadOnlyList<Word> words, bool throws = false)
        {
            _words = words;
            _throws = throws;
        }

        public IReadOnlyList<Word> GetAllWords()
        {
            if (_throws) throw new InvalidOperationException("Fallo simulado de la base de datos");
            return _words;
        }
    }
}
