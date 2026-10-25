using System.Collections.Generic;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Wavelength;
using Gamespot.Games.Wavelength.Data;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;
using UnityEngine;

namespace Gamespot.Tests.EditMode.Games.Wavelength
{
    public sealed class WavelengthDialAndDataTests
    {
        private GameObject _canvas;
        private WavelengthDial _dial;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            _dial = WavelengthDial.Create(_canvas.transform);
            var rect = (RectTransform)_dial.transform;
            rect.sizeDelta = new Vector2(400f, 300f);
            _dial.Setup(DialMode.Guess, 0.5, 0.5f);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_canvas);

        [Test]
        public void Dial_PosicionesDeIzquierdaArribaYDerecha()
        {
            var r = 100f;
            Assert.AreEqual(0f, _dial.PositionFromLocalPoint(_dial.LocalPointOf(0f, r)), 1e-4);
            Assert.AreEqual(0.5f, _dial.PositionFromLocalPoint(_dial.LocalPointOf(0.5f, r)), 1e-4);
            Assert.AreEqual(1f, _dial.PositionFromLocalPoint(_dial.LocalPointOf(1f, r)), 1e-4);
            Assert.AreEqual(0.25f, _dial.PositionFromLocalPoint(_dial.LocalPointOf(0.25f, 50f)), 1e-4);
        }

        [Test]
        public void Dial_ToqueBajoLaBase_VaAlExtremoMasCercano()
        {
            var leftBelow = _dial.LocalPointOf(0f, 100f) + new Vector2(0f, -30f);
            var rightBelow = _dial.LocalPointOf(1f, 100f) + new Vector2(0f, -30f);
            Assert.AreEqual(0f, _dial.PositionFromLocalPoint(leftBelow), "En el Flutter esto saltaba al polo derecho");
            Assert.AreEqual(1f, _dial.PositionFromLocalPoint(rightBelow));
        }

        [Test]
        public void Dial_TieneCanvasRendererYGeneraMalla()
        {
            Assert.IsNotNull(_dial.GetComponent<CanvasRenderer>(), "Sin CanvasRenderer el dial no se pinta");
            var vh = new UnityEngine.UI.VertexHelper();
            var populate = typeof(UnityEngine.UI.Graphic).GetMethod("OnPopulateMesh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(UnityEngine.UI.VertexHelper) }, null);
            populate.Invoke(_dial, new object[] { vh });
            Assert.Greater(vh.currentVertCount, 100, "Arco, marcas y aguja");
        }

        [Test]
        public void Dial_SoloSeMueveEnModoAdivinar()
        {
            var changes = new List<float>();
            _dial.NeedleChanged += changes.Add;
            _dial.SetNeedle(0.8f);
            Assert.AreEqual(0.8f, _dial.Needle);
            _dial.Setup(DialMode.Reveal, 0.3, 0.8f);
            _dial.SetNeedle(0.1f);
            Assert.AreEqual(0.8f, _dial.Needle, "En el revelado la aguja no se mueve");
            Assert.AreEqual(0f, _dial.RevealProgress);
            CollectionAssert.AreEqual(new[] { 0.8f }, changes);
        }

        [Test]
        public void Espectros_SeedYRepositorio()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new WavelengthGame() }, new ResourcesSeedSource());
                var all = new SpectrumRepository(db).GetAll();
                Assert.That(all.Count, Is.GreaterThanOrEqualTo(70));
                Assert.AreEqual("frío", all[0].Left);
                Assert.AreEqual("caliente", all[0].Right);
                Assert.AreEqual(0, WavelengthSchema.SeedIfEmpty(db, new ResourcesSeedSource()), "Idempotente");
            }
        }

        [Test]
        public void Espectros_SeedMalFormado_Lanza()
        {
            Assert.Throws<System.FormatException>(() => WavelengthSchema.Parse("[{\"izquierda\": \"a\"}]"));
        }
    }
}
