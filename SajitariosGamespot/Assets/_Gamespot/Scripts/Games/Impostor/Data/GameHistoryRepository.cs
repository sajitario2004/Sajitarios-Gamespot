using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Impostor.Domain;

namespace Gamespot.Games.Impostor.Data
{
    /// <summary>
    /// Historial de partidas del Impostor (port de <c>GameHistoryRepository</c>). Los jugadores se guardan en
    /// <c>players_json</c> como <c>[{"name": "…", "role": "impostor"|"palabra"}]</c>.
    /// </summary>
    public sealed class GameHistoryRepository
    {
        private sealed class Row
        {
            public long Id { get; set; }
            public long CreatedAt { get; set; }
            public string Word { get; set; }
            public string Hint { get; set; }
            public int NPlayers { get; set; }
            public int NImpostors { get; set; }
            public bool HintEnabled { get; set; }
            public string PlayersJson { get; set; }
        }

        private readonly IDb _db;
        private readonly Func<DateTimeOffset> _now;

        public GameHistoryRepository(IDb db, Func<DateTimeOffset> now = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _now = now ?? (() => DateTimeOffset.UtcNow);
        }

        /// <summary>Guarda una partida terminada y devuelve su id.</summary>
        public long InsertFromSession(GameSession session, bool hintEnabled)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var players = session.RevealOrder.Select(p => new GameRecordPlayer(p.Name, session.IsImpostor(p))).ToList();
            _db.Execute("INSERT INTO game_history (created_at, word, hint, n_players, n_impostors, hint_enabled, players_json) VALUES (?, ?, ?, ?, ?, ?, ?)",
                _now().ToUnixTimeMilliseconds(), session.Word.Text, session.Hint, players.Count, session.ImpostorCount, hintEnabled ? 1 : 0,
                PlayersToJson(players));
            return _db.LastInsertRowId;
        }

        /// <summary>Partidas, la más reciente primero.</summary>
        public List<GameRecord> GetAll()
        {
            var rows = _db.Query<Row>("SELECT id AS Id, created_at AS CreatedAt, word AS Word, hint AS Hint, n_players AS NPlayers, " +
                                      "n_impostors AS NImpostors, hint_enabled AS HintEnabled, players_json AS PlayersJson " +
                                      "FROM game_history ORDER BY created_at DESC, id DESC");
            return rows.Select(r => new GameRecord(r.Id, DateTimeOffset.FromUnixTimeMilliseconds(r.CreatedAt).LocalDateTime, r.Word, r.Hint,
                r.NPlayers, r.NImpostors, r.HintEnabled, PlayersFromJson(r.PlayersJson))).ToList();
        }

        public long Count() => _db.Scalar<long>("SELECT COUNT(*) FROM game_history");

        public void DeleteAll() => _db.Execute("DELETE FROM game_history");

        public static string PlayersToJson(IEnumerable<GameRecordPlayer> players)
        {
            var sb = new StringBuilder("[");
            var first = true;
            foreach (var p in players)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append("{\"name\":");
                JsonWriter.AppendString(sb, p.Name);
                sb.Append(",\"role\":\"").Append(p.WasImpostor ? "impostor" : "palabra").Append("\"}");
            }
            return sb.Append(']').ToString();
        }

        public static List<GameRecordPlayer> PlayersFromJson(string json)
        {
            var result = new List<GameRecordPlayer>();
            if (string.IsNullOrEmpty(json)) return result;
            foreach (var item in Json.ParseArray(json))
            {
                if (item is Dictionary<string, object> map && map.TryGetValue("name", out var name) && name is string n)
                {
                    result.Add(new GameRecordPlayer(n, map.TryGetValue("role", out var role) && (role as string) == "impostor"));
                }
            }
            return result;
        }
    }
}
