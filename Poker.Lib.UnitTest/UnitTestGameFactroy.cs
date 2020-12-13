using NUnit.Framework;

namespace Poker.Lib.UnitTest
{
    public class GameFactoryTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanCreateNewGame()
        {
            //assmble
            IPokerGame game;
            string[] playerNames = new string[5] {
                "Player1",
                "Player2",
                "Player3",
                "Player4",
                "Player5",};
            //act
            game = GameFactory.NewGame(playerNames);
            //assert
            for(var i = 0; i < playerNames.Length; i++)
            {
                Assert.AreEqual(playerNames[i], game.Players[i].Name);
            }
        }

        [Test]
        public void CanSaveAndLoadGame()
        {
            //assemble
            IPokerGame IGame;
            PokerGame game;
            string[] playerNames = new string[5] {
                "Player1",
                "Player2",
                "Player3",
                "Player4",
                "Player5",};
            IGame = GameFactory.NewGame(playerNames); 
            game = (PokerGame)IGame;
            game.gameAlive = true;
            int assignedWins = 0;
            foreach(Player player in game.Players)
            {
                player.Wins = assignedWins;
                assignedWins++;
            }
            //act
            game.SaveGameAndExit("TestSaveGameFile");
            GameFactory.LoadGame("TestSaveGameFile");
            //assert
            Assert.IsFalse(game.gameAlive);
            for(var i = 0; i < game.Players.Length; i++)
            {
                Assert.AreEqual(game.Players[i].Wins, i);
            }
        }
    }
}