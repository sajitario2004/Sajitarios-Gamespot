using Gamespot.Core;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Core
{
    public sealed class NameNormalizerTests
    {
        [TestCase("  Nacho  ", "Nacho")]
        [TestCase("Nacho   López", "Nacho López")]
        [TestCase("\tAna \n María ", "Ana María")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void Normalize_RecortaYColapsaEspacios(string input, string expected)
        {
            Assert.AreEqual(expected, NameNormalizer.Normalize(input));
        }

        [Test]
        public void DuplicateKey_NoDistingueMayusculasNiEspacios()
        {
            Assert.AreEqual(NameNormalizer.DuplicateKey("Nacho López"), NameNormalizer.DuplicateKey("  nacho   LÓPEZ "));
        }
    }
}
