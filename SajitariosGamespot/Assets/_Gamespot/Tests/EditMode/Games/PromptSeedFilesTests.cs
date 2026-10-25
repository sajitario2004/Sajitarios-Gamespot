using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Shared;
using Gamespot.Games.VerdadReto;
using Gamespot.Games.VerdadReto.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games
{
    /// <summary>
    /// Seeds de los juegos de cartas de la fase 13: se leen, no tienen repetidas, llegan a los mínimos por tipo y nivel
    /// y el picante no habla de menores (todo el contenido +18 es entre adultos que consienten).
    /// </summary>
    public sealed class PromptSeedFilesTests
    {
        private static readonly ResourcesSeedSource Seeds = new ResourcesSeedSource();

        /// <summary>Palabras que no pueden salir en una carta picante.</summary>
        private static readonly Regex Minors = new Regex(@"\b(menor|menores|niñ[oa]s?|crío|cría|colegio|instituto|adolescente|bebé|pequeñ[oa])\b",
            RegexOptions.IgnoreCase);

        private static readonly Regex Placeholder = new Regex(@"\{[^}]*\}");

        internal static void CheckCommon(IReadOnlyList<PromptCard> cards, params string[] allowedPlaceholders)
        {
            var keys = cards.Select(c => (NameNormalizer.DuplicateKey(c.Text).TrimEnd('.', '!', '?', '…', ' '),
                NameNormalizer.DuplicateKey(c.TextB ?? string.Empty).TrimEnd('.', '!', '?', '…', ' '), c.Kind)).ToList();
            var duplicates = keys.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key.Item1).ToList();
            CollectionAssert.IsEmpty(duplicates, "Sin repetidas");
            foreach (var card in cards)
            {
                foreach (Match m in Placeholder.Matches(card.Text + " " + card.TextB))
                {
                    CollectionAssert.Contains(allowedPlaceholders, m.Value, card.Text);
                }
                if (card.Level == PromptLevel.Picante) Assert.IsFalse(Minors.IsMatch(card.Text + " " + card.TextB), card.Text);
                Assert.That(card.Text.Length, Is.LessThanOrEqualTo(160), card.Text);
            }
        }

        [Test]
        public void VerdadReto_BancosGrandesEnLosDosNiveles()
        {
            var cards = PromptCardRepository.ParseSeed(Seeds.Load(VerdadRetoSchema.SeedName));
            CheckCommon(cards, VerdadRetoFlowController.Otro);
            CollectionAssert.IsSubsetOf(cards.Select(c => c.Kind).Distinct(), VerdadRetoKinds.All);
            foreach (var kind in new[] { VerdadRetoKinds.Verdad, VerdadRetoKinds.Reto })
            {
                foreach (var level in PromptLevels.All)
                {
                    Assert.That(cards.Count(c => c.Kind == kind && c.Level == level), Is.GreaterThanOrEqualTo(120), $"{kind} {level}");
                }
            }
            foreach (var level in PromptLevels.All)
            {
                Assert.That(cards.Count(c => c.Kind == VerdadRetoKinds.Castigo && c.Level == level), Is.GreaterThanOrEqualTo(25), $"castigo {level}");
            }
            Assert.That(cards.Count(c => c.Text.Contains(VerdadRetoFlowController.Otro)), Is.GreaterThanOrEqualTo(40), "Muchas nombran a otro jugador");
            // Los retos picantes que tocan a otro piden su permiso.
            var touching = new Regex(@"\b(besa|beso|bésale|lame|lámele|muerde|muérdele|toca|acaricia|azote|chupetón|siéntate|quítale|pico)\b", RegexOptions.IgnoreCase);
            foreach (var card in cards.Where(c => c.Kind == VerdadRetoKinds.Reto && c.Level == PromptLevel.Picante && c.Text.Contains("{otro}")
                                                  && touching.IsMatch(c.Text)))
            {
                StringAssert.Contains("(si", card.Text, card.Text);
            }
        }
    }
}
