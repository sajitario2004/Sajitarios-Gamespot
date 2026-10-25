using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Games.Impostor.Domain
{
    /// <summary>Un jugador de una partida guardada y si fue impostor.</summary>
    public sealed class GameRecordPlayer
    {
        public GameRecordPlayer(string name, bool wasImpostor)
        {
            Name = name;
            WasImpostor = wasImpostor;
        }

        public string Name { get; }
        public bool WasImpostor { get; }
    }

    /// <summary>
    /// Partida terminada del historial (port de <c>GameRecord</c>). <see cref="NImpostors"/> es el número real de
    /// impostores (puede ser 0 o todos).
    /// </summary>
    public sealed class GameRecord
    {
        public GameRecord(long id, DateTime createdAt, string word, string hint, int nPlayers, int nImpostors, bool hintEnabled,
            IReadOnlyList<GameRecordPlayer> players)
        {
            Id = id;
            CreatedAt = createdAt;
            Word = word;
            Hint = hint;
            NPlayers = nPlayers;
            NImpostors = nImpostors;
            HintEnabled = hintEnabled;
            Players = players ?? Array.Empty<GameRecordPlayer>();
        }

        public long Id { get; }

        /// <summary>Fecha y hora local de la partida.</summary>
        public DateTime CreatedAt { get; }

        public string Word { get; }
        public string Hint { get; }
        public int NPlayers { get; }
        public int NImpostors { get; }
        public bool HintEnabled { get; }
        public IReadOnlyList<GameRecordPlayer> Players { get; }

        /// <summary>Crea el registro de una sesión terminada (orden de jugadores = orden de revelación).</summary>
        public static GameRecord FromSession(GameSession session, bool hintEnabled, DateTime createdAt)
        {
            var players = session.RevealOrder.Select(p => new GameRecordPlayer(p.Name, session.IsImpostor(p))).ToList();
            return new GameRecord(0, createdAt, session.Word.Text, session.Hint, players.Count, session.ImpostorCount, hintEnabled, players);
        }
    }

    /// <summary>
    /// Estadísticas del historial (como las calculaba la pantalla del Flutter, en código):
    /// total de partidas, palabra más repetida y cuántas veces fue impostor cada jugador.
    /// </summary>
    public sealed class ImpostorStats
    {
        private ImpostorStats(int total, string mostRepeatedWord, int mostRepeatedCount, IReadOnlyList<(string name, int count)> ranking)
        {
            Total = total;
            MostRepeatedWord = mostRepeatedWord;
            MostRepeatedCount = mostRepeatedCount;
            ImpostorRanking = ranking;
        }

        public int Total { get; }

        /// <summary>Palabra más repetida (nula sin partidas), con la grafía de la partida más reciente.</summary>
        public string MostRepeatedWord { get; }
        public int MostRepeatedCount { get; }

        /// <summary>Jugadores que han sido impostor alguna vez: más veces primero, empate por nombre (minúsculas).</summary>
        public IReadOnlyList<(string name, int count)> ImpostorRanking { get; }

        /// <summary><paramref name="records"/> en el orden del repositorio (la más reciente primero).</summary>
        public static ImpostorStats Compute(IReadOnlyList<GameRecord> records)
        {
            if (records == null || records.Count == 0) return new ImpostorStats(0, null, 0, Array.Empty<(string, int)>());

            // Palabra más repetida: agrupada en minúsculas; empate → la clave alfabéticamente menor.
            var groups = new Dictionary<string, (int count, string display)>();
            foreach (var record in records)
            {
                var key = record.Word.ToLowerInvariant();
                if (groups.TryGetValue(key, out var group)) groups[key] = (group.count + 1, group.display);
                else groups[key] = (1, record.Word); // la primera vista es la más reciente
            }
            var best = groups.OrderByDescending(g => g.Value.count).ThenBy(g => g.Key, StringComparer.Ordinal).First();

            // Ranking de impostores por nombre exacto.
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var record in records)
            {
                foreach (var player in record.Players.Where(p => p.WasImpostor))
                {
                    counts[player.Name] = counts.TryGetValue(player.Name, out var c) ? c + 1 : 1;
                }
            }
            var ranking = counts.OrderByDescending(p => p.Value)
                .ThenBy(p => p.Key.ToLowerInvariant(), StringComparer.Ordinal)
                .Select(p => (p.Key, p.Value))
                .ToList();

            return new ImpostorStats(records.Count, best.Value.display, best.Value.count, ranking);
        }
    }
}
