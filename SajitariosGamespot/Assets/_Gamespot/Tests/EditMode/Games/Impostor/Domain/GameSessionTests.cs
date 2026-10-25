using System;
using System.Collections.Generic;
using Gamespot.Games.Impostor.Domain;
using NUnit.Framework;
using static Gamespot.Tests.EditMode.Games.Impostor.ImpostorTestData;

namespace Gamespot.Tests.EditMode.Games.Impostor
{
    public sealed class GameSessionTests
    {
        private static readonly Word Playa = new Word("playa", "arena");

        [Test]
        public void FaltaUnRol_Lanza()
        {
            var players = Players("A", "B", "C");
            var roles = new Dictionary<Player, Role> { [players[0]] = Role.Impostor, [players[1]] = Role.Palabra };
            Assert.Throws<ArgumentException>(() => new GameSession(Playa, players, roles));
        }

        [Test]
        public void AsignacionesDeMas_Lanza()
        {
            var players = Players("A", "B", "C");
            var roles = new Dictionary<Player, Role>
            {
                [players[0]] = Role.Impostor, [players[1]] = Role.Palabra, [players[2]] = Role.Palabra, [new Player("X")] = Role.Palabra
            };
            Assert.Throws<ArgumentException>(() => new GameSession(Playa, players, roles));
        }

        [Test]
        public void ConsultasDeRol()
        {
            var players = Players("Nacho", "Iker", "Lucía");
            var roles = new Dictionary<Player, Role> { [players[0]] = Role.Impostor, [players[1]] = Role.Palabra, [players[2]] = Role.Palabra };
            var session = new GameSession(Playa, players, roles);

            Assert.AreEqual("arena", session.Hint);
            CollectionAssert.AreEqual(players, session.RevealOrder);
            Assert.AreEqual(Role.Impostor, session.RoleOf(new Player("Nacho")));
            Assert.AreEqual(Role.Palabra, session.RoleOf(players[1]));
            Assert.IsNull(session.RoleOf(new Player("Ajeno")));
            Assert.IsTrue(session.IsImpostor(players[0]));
            Assert.IsFalse(session.IsImpostor(players[2]));
            CollectionAssert.AreEqual(new[] { players[0] }, session.Impostores);
            Assert.AreEqual(1, session.ImpostorCount);
        }

        [Test]
        public void PalabraYPista_SeRecortanYSonObligatorias()
        {
            var word = new Word("  pirata ", " barco  ");
            Assert.AreEqual("pirata", word.Text);
            Assert.AreEqual("barco", word.Hint);
            Assert.Throws<ArgumentException>(() => new Word(" ", "barco"));
            Assert.Throws<ArgumentException>(() => new Word("pirata", ""));
            Assert.AreEqual(new Word("playa", "arena"), new Word("playa", "arena"));
        }

        [Test]
        public void Jugador_IgualdadPorNombreExacto()
        {
            Assert.AreEqual(new Player("Ana"), new Player("Ana"));
            Assert.AreNotEqual(new Player("Ana"), new Player("ana"));
        }
    }
}
