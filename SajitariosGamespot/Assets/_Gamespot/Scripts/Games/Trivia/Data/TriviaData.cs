using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Trivia.Domain;

namespace Gamespot.Games.Trivia.Data
{
    /// <summary>
    /// Tablas de la trivia: <c>trivia_questions</c> (opciones en JSON) y <c>trivia_winners</c> (victorias por nombre,
    /// sin distinguir mayúsculas). Seed: <c>trivia_questions.json</c> (<c>[{tematica, difficulty, enunciado, opciones[4], correcta}]</c>).
    /// </summary>
    public static class TriviaSchema
    {
        public const string QuestionsTable = "trivia_questions";
        public const string SeedName = "trivia_questions";

        public static void Create(IDb db)
        {
            db.Execute("CREATE TABLE IF NOT EXISTS trivia_questions (" +
                       "id INTEGER PRIMARY KEY AUTOINCREMENT, tematica_id TEXT NOT NULL, difficulty TEXT NOT NULL, enunciado TEXT NOT NULL, " +
                       "options_json TEXT NOT NULL, correct_index INTEGER NOT NULL, is_seed INTEGER NOT NULL DEFAULT 0)");
            db.Execute("CREATE INDEX IF NOT EXISTS idx_trivia_questions_tema_diff ON trivia_questions(tematica_id, difficulty)");
            db.Execute("CREATE TABLE IF NOT EXISTS trivia_winners (" +
                       "id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL UNIQUE COLLATE NOCASE, wins INTEGER NOT NULL DEFAULT 0)");
        }

        public static int SeedIfEmpty(IDb db, ISeedSource seeds)
        {
            if (!AppDatabase.IsEmpty(db, QuestionsTable)) return 0;
            var inserted = 0;
            foreach (var q in Parse(seeds.Load(SeedName)))
            {
                inserted += db.Execute("INSERT INTO trivia_questions (tematica_id, difficulty, enunciado, options_json, correct_index, is_seed) " +
                                       "VALUES (?, ?, ?, ?, ?, 1)", q.TematicaId, q.Difficulty.StorageName(), q.Enunciado, OptionsToJson(q.Options), q.CorrectIndex);
            }
            return inserted;
        }

        /// <summary>Lee y valida el seed (lanza <see cref="FormatException"/> con entradas mal formadas).</summary>
        public static List<Question> Parse(string json)
        {
            var result = new List<Question>();
            foreach (var item in Json.ParseArray(json))
            {
                if (!(item is Dictionary<string, object> map)) throw new FormatException("Entrada de trivia no válida");
                try
                {
                    var options = ((List<object>)map["opciones"]).Select(o => ((string)o).Trim()).ToList();
                    result.Add(new Question(0, ((string)map["tematica"]).Trim(), Difficulties.FromStorageName((string)map["difficulty"]),
                        ((string)map["enunciado"]).Trim(), options, Convert.ToInt32(map["correcta"])));
                }
                catch (Exception e) when (!(e is FormatException))
                {
                    throw new FormatException($"Pregunta de trivia no válida: {e.Message}");
                }
            }
            return result;
        }

        public static string OptionsToJson(IEnumerable<string> options)
        {
            var sb = new StringBuilder("[");
            var first = true;
            foreach (var o in options)
            {
                if (!first) sb.Append(',');
                first = false;
                JsonWriter.AppendString(sb, o);
            }
            return sb.Append(']').ToString();
        }
    }

    /// <summary>Preguntas por temáticas y dificultad (port de <c>QuestionRepository</c>).</summary>
    public sealed class QuestionRepository : ITriviaQuestionSource
    {
        private sealed class Row
        {
            public long Id { get; set; }
            public string TematicaId { get; set; }
            public string Difficulty { get; set; }
            public string Enunciado { get; set; }
            public string OptionsJson { get; set; }
            public int CorrectIndex { get; set; }
        }

        private readonly IDb _db;

        public QuestionRepository(IDb db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public long Count() => _db.Scalar<long>("SELECT COUNT(*) FROM trivia_questions");

        public long CountBy(string tematicaId, Difficulty difficulty) =>
            _db.Scalar<long>("SELECT COUNT(*) FROM trivia_questions WHERE tematica_id = ? AND difficulty = ?", tematicaId, difficulty.StorageName());

        /// <summary>Preguntas de esas temáticas y esa dificultad, por id. Sin temáticas, vacío.</summary>
        public IReadOnlyList<Question> GetPool(IReadOnlyCollection<string> tematicaIds, Difficulty difficulty)
        {
            if (tematicaIds == null || tematicaIds.Count == 0) return Array.Empty<Question>();
            var ids = tematicaIds.ToList();
            var placeholders = string.Join(", ", ids.Select(_ => "?"));
            var args = ids.Cast<object>().Concat(new object[] { difficulty.StorageName() }).ToArray();
            var rows = _db.Query<Row>("SELECT id AS Id, tematica_id AS TematicaId, difficulty AS Difficulty, enunciado AS Enunciado, " +
                                      $"options_json AS OptionsJson, correct_index AS CorrectIndex FROM trivia_questions WHERE tematica_id IN ({placeholders}) " +
                                      "AND difficulty = ? ORDER BY id ASC", args);
            return rows.Select(r => new Question(r.Id, r.TematicaId, Difficulties.FromStorageName(r.Difficulty), r.Enunciado,
                Json.ParseArray(r.OptionsJson).Select(o => (string)o).ToList(), r.CorrectIndex)).ToList();
        }
    }

    /// <summary>Victorias por nombre (port de <c>WinnerRepository</c>): un nombre distinto empieza de cero.</summary>
    public sealed class WinnerRepository : ITriviaWinnerSink
    {
        public sealed class Entry
        {
            public string Name { get; set; }
            public int Wins { get; set; }
        }

        private readonly IDb _db;

        public WinnerRepository(IDb db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public void IncrementWins(string name)
        {
            var clean = name?.Trim() ?? string.Empty;
            if (clean.Length == 0) return;
            _db.InTransaction(() =>
            {
                _db.Execute("INSERT OR IGNORE INTO trivia_winners (name, wins) VALUES (?, 0)", clean);
                _db.Execute("UPDATE trivia_winners SET wins = wins + 1 WHERE name = ? COLLATE NOCASE", clean);
            });
        }

        public int GetWins(string name) =>
            _db.Query<Entry>("SELECT name AS Name, wins AS Wins FROM trivia_winners WHERE name = ? COLLATE NOCASE", name?.Trim() ?? string.Empty)
                .Select(e => e.Wins).FirstOrDefault();

        /// <summary>Más victorias primero; empate por nombre sin mayúsculas.</summary>
        public List<Entry> GetAllRanked() =>
            _db.Query<Entry>("SELECT name AS Name, wins AS Wins FROM trivia_winners ORDER BY wins DESC, name COLLATE NOCASE ASC");
    }
}
