using Gamespot.Games.YoNunca.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.YoNunca
{
    /// <summary>V059q: las frases propias empiezan siempre por «Yo nunca».</summary>
    public sealed class NeverStatementTextTests
    {
        [Test]
        public void SiFaltaElPrefijoSeAnade()
        {
            Assert.AreEqual("Yo nunca he ido a Ibiza", NeverStatementText.Normalize("  He ido   a Ibiza ", "Yo nunca"));
            Assert.AreEqual("Yo nunca me he liado con un camarero", NeverStatementText.Normalize("me he liado con un camarero", "Yo nunca"));
        }

        [Test]
        public void SiYaEstaSeRespetaConSuGrafia()
        {
            Assert.AreEqual("Yo nunca he ido a Ibiza.", NeverStatementText.Normalize("yo NUNCA he ido a Ibiza.", "Yo nunca"));
            Assert.AreEqual("Yo nunca he ido a Ibiza", NeverStatementText.Normalize("Yo nunca, he ido a Ibiza", "Yo nunca"));
        }

        [Test]
        public void VaciaSoloPrefijoOLarga()
        {
            Assert.AreEqual(string.Empty, NeverStatementText.Normalize("   ", "Yo nunca"));
            Assert.AreEqual(string.Empty, NeverStatementText.Normalize("Yo nunca…", "Yo nunca"));
            Assert.AreEqual(NeverStatementText.MaxLength, NeverStatementText.Normalize(new string('a', 300), "Yo nunca").Length);
        }
    }
}
