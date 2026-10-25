using System.Linq;
using Gamespot.Core.Runtime.Localization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gamespot.Tests.EditMode.Localization
{
    public sealed class LocTests
    {
        private string _savedPreference;
        private bool _hadPreference;

        [SetUp]
        public void SetUp()
        {
            _hadPreference = PlayerPrefs.HasKey(Loc.PreferenceKey);
            _savedPreference = PlayerPrefs.GetString(Loc.PreferenceKey, string.Empty);
            Loc.SetPreference(Loc.Spanish);
        }

        [TearDown]
        public void TearDown()
        {
            // Deja la preferencia del Editor como estaba.
            if (_hadPreference) PlayerPrefs.SetString(Loc.PreferenceKey, _savedPreference);
            else PlayerPrefs.DeleteKey(Loc.PreferenceKey);
            PlayerPrefs.Save();
            Loc.Initialize();
        }

        [Test]
        public void EspanolEIngles_TienenLasMismasClaves()
        {
            var es = Loc.KeysOf(Loc.Spanish).OrderBy(k => k).ToList();
            var en = Loc.KeysOf(Loc.English).OrderBy(k => k).ToList();
            Assert.That(es.Count, Is.GreaterThanOrEqualTo(305), "Al menos las 305 claves del Flutter");
            CollectionAssert.AreEqual(es, en, "Paridad de claves es/en");
        }

        [Test]
        public void TextoSimpleYConArgumentos()
        {
            Assert.AreEqual("Sajitarios Gamespot", Loc.T("appTitle"));
            Assert.AreEqual("Jugador 3", Loc.T("jugadorNumero", ("n", 3)));
        }

        [Test]
        public void Plurales()
        {
            Assert.AreEqual("1 impostor", Loc.T("impostoresContador", ("n", 1)));
            Assert.AreEqual("3 impostores", Loc.T("impostoresContador", ("n", 3)));
        }

        [Test]
        public void CambiarAIngles_SeGuardaYAvisa()
        {
            var avisos = 0;
            Loc.LanguageChanged += () => avisos++;
            Loc.SetPreference(Loc.English);
            Assert.AreEqual(Loc.English, Loc.Language);
            Assert.AreEqual(Loc.English, PlayerPrefs.GetString(Loc.PreferenceKey));
            Assert.AreEqual("3 impostors", Loc.T("impostoresContador", ("n", 3)));
            Assert.AreEqual(1, avisos);
        }

        [Test]
        public void IdiomaDelSistema_BorraLaPreferencia()
        {
            Loc.SetPreference(null);
            Assert.IsFalse(PlayerPrefs.HasKey(Loc.PreferenceKey));
            Assert.IsNull(Loc.Preference);
        }

        [Test]
        public void ClaveQueNoExiste_SeMuestraEntreCorchetes()
        {
            LogAssert.Expect(LogType.Warning, "[Loc] Falta la clave 'claveQueNoExiste'");
            Assert.AreEqual("[claveQueNoExiste]", Loc.T("claveQueNoExiste"));
        }

        [Test]
        public void ClavesNuevasDeUnity()
        {
            Assert.AreEqual("Tréboles", Loc.T("paloTreboles"));
            Assert.AreEqual("La Bomba", Loc.T("bombaMenuTitulo"));
            Assert.AreEqual("2.5 puntos/ronda", Loc.T("wavelengthPuntosPorRonda", ("avg", "2.5")));
        }
    }
}
