using System;
using System.Collections.Generic;
using Gamespot.Core;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Core
{
    public sealed class JsonTests
    {
        [Test]
        public void LeeObjetosListasYEscalares()
        {
            var root = Json.ParseObject("{\"a\": 1, \"b\": [true, false, null], \"c\": \"hola\", \"d\": -2.5e1}");
            Assert.AreEqual(1.0, root["a"]);
            CollectionAssert.AreEqual(new object[] { true, false, null }, (List<object>)root["b"]);
            Assert.AreEqual("hola", root["c"]);
            Assert.AreEqual(-25.0, root["d"]);
        }

        [Test]
        public void LeeEscapesYTildes()
        {
            var list = Json.ParseArray("[\"Tr\\u00e9boles\", \"l\\u00ednea\\nnueva\", \"comillas \\\" y \\\\\", \"ñandú ¿qué?\"]");
            Assert.AreEqual("Tréboles", list[0]);
            Assert.AreEqual("línea\nnueva", list[1]);
            Assert.AreEqual("comillas \" y \\", list[2]);
            Assert.AreEqual("ñandú ¿qué?", list[3]);
        }

        [Test]
        public void ObjetoYListaVacios()
        {
            Assert.AreEqual(0, Json.ParseObject("{ }").Count);
            Assert.AreEqual(0, Json.ParseArray(" [ ] ").Count);
        }

        [TestCase("{\"a\": }")]
        [TestCase("[1, 2")]
        [TestCase("{\"a\": 1} basura")]
        [TestCase("\"sin cerrar")]
        [TestCase("")]
        public void JsonInvalido_Lanza(string text)
        {
            Assert.Throws<FormatException>(() => Json.Parse(text));
        }

        [Test]
        public void RaizEquivocada_Lanza()
        {
            Assert.Throws<FormatException>(() => Json.ParseObject("[1]"));
            Assert.Throws<FormatException>(() => Json.ParseArray("{}"));
        }
    }
}
