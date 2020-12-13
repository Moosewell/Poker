using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Timers;

namespace Poker.Lib.UnitTest
{
    public class PokerGameTests
    {
        static PokerGame gameEventStorage;
        public void Interupt(Object source, ElapsedEventArgs e)
        {
            gameEventStorage.gameAlive = false;
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanExitGame()
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
            //act
            game.Exit();
            //assert
            Assert.IsFalse(game.gameAlive);
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

        [Test]
        public void CanRunGame()
        {
            //assemble
            IPokerGame IGame;
            PokerGame game;
            Timer timer = new Timer(1000);
            string[] playerNames = new string[5] {
                "Player1",
                "Player2",
                "Player3",
                "Player4",
                "Player5",};
            IGame = GameFactory.NewGame(playerNames); 
            game = (PokerGame)IGame;
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            game.NewDeal += TestMethods.OnNewDeal;
            game.SelectCardsToDiscard += TestMethods.OnSelectCardsToDiscard;
            game.ShowAllHands += TestMethods.OnShowAllHands;
            //act
            gameEventStorage = (PokerGame)game;
            timer.Elapsed += Interupt;
            timer.AutoReset = true;
            timer.Enabled = true;
            game.RunGame();
            timer.Stop();
            timer.Dispose();
            int[] wins = game.Players.Select(element => element.Wins).ToArray();
            //assert
            CollectionAssert.AreNotEqual(new int[5]{0,0,0,0,0}, wins);
        }

        [Test]
        public void CanTestDraw()
        {
            for(var i = 0; i < 10000; i++)
            {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "Player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<ICard[]> AllHands = new List<ICard[]>();

            ICard[] hand = new ICard[5] {
                new Card((Suite)TestMethods.RandomSuite(), (Rank)TestMethods.RandomRank()),
                new Card((Suite)TestMethods.RandomSuite(), (Rank)TestMethods.RandomRank()),
                new Card((Suite)TestMethods.RandomSuite(), (Rank)TestMethods.RandomRank()),
                new Card((Suite)TestMethods.RandomSuite(), (Rank)TestMethods.RandomRank()),
                new Card((Suite)TestMethods.RandomSuite(), (Rank)TestMethods.RandomRank())};
            foreach(Player player in game.Players)
            {
                player.DrawCards(hand);
                player.SortHand();
                player.IdentifyHand();
                AllHands.Add(player.Hand);
            }
            //act
            game.HandleResult(game.Players, dealer);
            //assert
            foreach(Player player in game.Players)
            {
                Assert.AreEqual(0, player.Wins);
            }
            }
        }


        [Test]
        public void CanHandleResultsForWorseHandWithEqualStraightFlush()
        {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            ICard[][] HandList = new ICard[5][] {
                new ICard[5] {
                    new Card((Suite)1,(Rank)9),
                    new Card((Suite)1,(Rank)10), 
                    new Card((Suite)1,(Rank)11), 
                    new Card((Suite)1,(Rank)12), 
                    new Card((Suite)1,(Rank)13)},
                new ICard[5] {
                    new Card((Suite)2,(Rank)8),
                    new Card((Suite)2,(Rank)9), 
                    new Card((Suite)2,(Rank)10), 
                    new Card((Suite)2,(Rank)11), 
                    new Card((Suite)2,(Rank)12)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)2),
                    new Card((Suite)1,(Rank)3), 
                    new Card((Suite)1,(Rank)4), 
                    new Card((Suite)1,(Rank)5), 
                    new Card((Suite)1,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)3,(Rank)2),
                    new Card((Suite)3,(Rank)3), 
                    new Card((Suite)3,(Rank)4), 
                    new Card((Suite)3,(Rank)5), 
                    new Card((Suite)3,(Rank)6)},
                new ICard[5] {
                    new Card((Suite)0,(Rank)5),
                    new Card((Suite)0,(Rank)6), 
                    new Card((Suite)0,(Rank)7), 
                    new Card((Suite)0,(Rank)8), 
                    new Card((Suite)0,(Rank)9)},
            };
            int index = 0;
            foreach(Player player in game.Players)
            {
                player.DrawCards(HandList[index]);
                player.SortHand();
                player.IdentifyHand();
                index++;
            }
            //act
            game.HandleResult(game.Players.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, game.Players[0].Wins);
        }

        [Test]
        public void CanHandleResultsForStraightFlush()
        {
            for(var i = 0; i < 1000; i++)
            {
            Dealer dealer = new Dealer();
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();

            #region Create Best Player

            Suite suite = (Suite)TestMethods.RandomSuite();
            int bestTempRank;
            do
            {
            bestTempRank = TestMethods.RandomRank();
            } while(bestTempRank == 1 || bestTempRank > 10);
            ICard[] hand = new ICard[5] 
            {new Card(suite, (Rank)bestTempRank), 
            new Card(suite, (Rank)bestTempRank + 1),
            new Card(suite, (Rank)bestTempRank + 2),
            new Card(suite, (Rank)bestTempRank + 3),
            new Card(suite, (Rank)bestTempRank + 4)};
            Player bestPlayer = new Player("BestPlayer");
            bestPlayer.DrawCards(hand);
            bestPlayer.SortHand();
            bestPlayer.IdentifyHand();
            AllPlayers.Add(bestPlayer);
            #endregion
        
            #region Create Worse Players
            foreach(Player player in game.Players)
            {
                suite = (Suite)TestMethods.RandomSuite();
                int tempRank;
                do
                {
                tempRank = TestMethods.RandomRank();
                } while(tempRank > bestTempRank);

                Player newPlayer = new Player("newPlayer");
                if(tempRank == bestTempRank)
                {
                    hand = new ICard[5] 
                    {new Card(suite, (Rank)2), 
                    new Card(suite, (Rank)3),
                    new Card(suite, (Rank)4),
                    new Card(suite, (Rank)5),
                    new Card(suite, (Rank)14)};
                    newPlayer.DrawCards(hand);
                }
                else
                {
                    hand = new ICard[5] 
                    {new Card(suite, (Rank)tempRank), 
                    new Card(suite, (Rank)tempRank + 1),
                    new Card(suite, (Rank)tempRank + 2),
                    new Card(suite, (Rank)tempRank + 3),
                    new Card(suite, (Rank)tempRank + 4)};
                    newPlayer.DrawCards(hand);
                }
                newPlayer.SortHand();
                newPlayer.IdentifyHand();
                AllPlayers.Add(newPlayer);
            }
            #endregion
            
            //act
            game.HandleResult(AllPlayers.ToArray(), dealer);
            //assert
            Assert.AreEqual(1, bestPlayer.Wins);
            }
        }
        [Test]
        public void CanHandleResultsForWorseHandWithEqualFourOfAKind()
        {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            ICard[][] HandList = new ICard[5][] {
                new ICard[5] {
                    new Card((Suite)2,(Rank)14),
                    new Card((Suite)1,(Rank)14), 
                    new Card((Suite)0,(Rank)14), 
                    new Card((Suite)1,(Rank)14), 
                    new Card((Suite)1,(Rank)8)},
                new ICard[5] {
                    new Card((Suite)3,(Rank)14),
                    new Card((Suite)2,(Rank)14), 
                    new Card((Suite)3,(Rank)14), 
                    new Card((Suite)4,(Rank)14), 
                    new Card((Suite)1,(Rank)5)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)8),
                    new Card((Suite)1,(Rank)8), 
                    new Card((Suite)1,(Rank)8), 
                    new Card((Suite)3,(Rank)8), 
                    new Card((Suite)2,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)2,(Rank)10),
                    new Card((Suite)3,(Rank)10), 
                    new Card((Suite)0,(Rank)10), 
                    new Card((Suite)2,(Rank)10), 
                    new Card((Suite)1,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)2),
                    new Card((Suite)3,(Rank)2), 
                    new Card((Suite)0,(Rank)2), 
                    new Card((Suite)0,(Rank)2), 
                    new Card((Suite)1,(Rank)14)},
            };
            int index = 0;
            foreach(Player player in game.Players)
            {
                player.DrawCards(HandList[index]);
                player.SortHand();
                player.IdentifyHand();
                index++;
            }
            //act
            game.HandleResult(game.Players.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, game.Players[0].Wins);
        }

        [Test]
                public void CanHandleResultsForWorseFourOfAKind()
        {
            for(var i = 0; i < 1000; i++)
            {
            Dealer dealer = new Dealer();
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();

            #region Create Best Player
            
            dealer = new Dealer();
            int bestRandomRank;
            do
            {
            bestRandomRank = TestMethods.RandomRank();
            } while (bestRandomRank == 2);
            var randomFOK = dealer.Deck.Where(card => (int)card.Rank == bestRandomRank).ToArray();
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != bestRandomRank).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player bestPlayer = new Player("Gustav");
            bestPlayer.DrawCards(randomFOK);
            bestPlayer.DrawCards(TestMethods.DealTopCards(1, smallTestDeck));
            bestPlayer.SortHand();
            bestPlayer.IdentifyHand();
            AllPlayers.Add(bestPlayer);
            #endregion
        
            #region Create Worse Players
            foreach(Player player in game.Players)
            {
            dealer = new Dealer();
            int randomRank;
            do
            {
            randomRank = TestMethods.RandomRank();
            } while (randomRank >= bestRandomRank);
            randomFOK = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player newPlayer = new Player("Gustav");
            newPlayer.DrawCards(randomFOK);
            newPlayer.DrawCards(TestMethods.DealTopCards(1, smallTestDeck));
            newPlayer.SortHand();
            newPlayer.IdentifyHand();
            AllPlayers.Add(newPlayer);
            }
                #endregion
            
            //act
            game.HandleResult(AllPlayers.ToArray(), dealer);
            //assert
            Assert.AreEqual(1, bestPlayer.Wins);
            }
        }

        [Test]
        public void CanHandleResultsForWorseHandWithEqualFullHouse()
        {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            ICard[][] HandList = new ICard[5][] {
                new ICard[5] {
                    new Card((Suite)2,(Rank)14),
                    new Card((Suite)1,(Rank)14), 
                    new Card((Suite)0,(Rank)14), 
                    new Card((Suite)1,(Rank)8), 
                    new Card((Suite)1,(Rank)8)},
                new ICard[5] {
                    new Card((Suite)3,(Rank)14),
                    new Card((Suite)2,(Rank)14), 
                    new Card((Suite)3,(Rank)14), 
                    new Card((Suite)4,(Rank)5), 
                    new Card((Suite)1,(Rank)5)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)5),
                    new Card((Suite)1,(Rank)5), 
                    new Card((Suite)1,(Rank)5), 
                    new Card((Suite)3,(Rank)14), 
                    new Card((Suite)2,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)2,(Rank)7),
                    new Card((Suite)3,(Rank)7), 
                    new Card((Suite)0,(Rank)7), 
                    new Card((Suite)2,(Rank)14), 
                    new Card((Suite)1,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)2),
                    new Card((Suite)3,(Rank)2), 
                    new Card((Suite)0,(Rank)2), 
                    new Card((Suite)0,(Rank)3), 
                    new Card((Suite)1,(Rank)3)},
            };
            int index = 0;
            foreach(Player player in game.Players)
            {
                player.DrawCards(HandList[index]);
                player.SortHand();
                player.IdentifyHand();
                index++;
            }
            //act
            game.HandleResult(game.Players.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, game.Players[0].Wins);
        }

        [Test]
                public void CanHandleResultsForFullHouse()
        {
            for(var i = 0; i < 1000; i++)
            {
            Dealer dealer = new Dealer();
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();

            #region Create Best Player

            dealer = new Dealer();
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for(var j = 0; j < 2; j++)
            {
                while(true)
                {
                    int tempNum = TestMethods.RandomRank();
                    if(!randomNumbers.Contains(tempNum) && tempNum > 3)
                    {
                        randomNumbers.Add(tempNum);
                        break;
                    }
                }
            }
            int bestRandomRank = randomNumbers[0]; 
            int randomRank1 = randomNumbers[0];
            int randomRank2 = randomNumbers[1];
            var randomPair1 = dealer.Deck.Where(card => (int)card.Rank == randomRank1).ToArray();
            var randomPair2 = dealer.Deck.Where(card => (int)card.Rank == randomRank2).ToArray();
            while (randomPair1.Length > 3)
            {
                int removeCard1 = random.Next(0, randomPair1.Length);
                randomPair1[removeCard1] = null;
                randomPair1 = randomPair1.Where(card => card != null).ToArray();
            }
            while (randomPair2.Length > 2)
            {
                int removeCard2 = random.Next(0, randomPair2.Length);
                randomPair2[removeCard2] = null;
                randomPair2 = randomPair2.Where(card => card != null).ToArray();
            }
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank1).ToArray();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank2).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player bestPlayer = new Player("Gustav");
            bestPlayer.DrawCards(randomPair1);
            bestPlayer.DrawCards(randomPair2);
            bestPlayer.SortHand();
            bestPlayer.IdentifyHand();
            AllPlayers.Add(bestPlayer);
            #endregion
        
            #region Create Worse Players
            foreach(Player player in game.Players)
            {
            dealer = new Dealer();
            randomNumbers = new List<int>();
            for(var j = 0; j < 2; j++)
            {
                while(true)
                {
                    int tempNum = TestMethods.RandomRank();
                    if(!randomNumbers.Contains(tempNum) && tempNum < bestRandomRank)
                    {
                        randomNumbers.Add(tempNum);
                        break;
                    }
                }
            }
            randomRank1 = randomNumbers[0];
            randomRank2 = randomNumbers[1];
            randomPair1 = dealer.Deck.Where(card => (int)card.Rank == randomRank1).ToArray();
            randomPair2 = dealer.Deck.Where(card => (int)card.Rank == randomRank2).ToArray();
            while (randomPair1.Length > 3)
            {
                int removeCard1 = random.Next(0, randomPair1.Length);
                randomPair1[removeCard1] = null;
                randomPair1 = randomPair1.Where(card => card != null).ToArray();
            }
            while (randomPair2.Length > 2)
            {
                int removeCard2 = random.Next(0, randomPair2.Length);
                randomPair2[removeCard2] = null;
                randomPair2 = randomPair2.Where(card => card != null).ToArray();
            }
            smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank1).ToArray();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank2).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player newPlayer = new Player("Gustav");
            newPlayer.DrawCards(randomPair1);
            newPlayer.DrawCards(randomPair2);
            newPlayer.SortHand();
            newPlayer.IdentifyHand();
            AllPlayers.Add(newPlayer);
            }
                #endregion
            
            //act
            game.HandleResult(AllPlayers.ToArray(), dealer);
            //assert
            Assert.AreEqual(1, bestPlayer.Wins);
            }
        }

        [Test]
        public void CanHandleResultsForWorseHandWithEqualFlush()
        {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            ICard[][] HandList = new ICard[5][] {
                new ICard[5] {
                    new Card((Suite)1,(Rank)11),
                    new Card((Suite)1,(Rank)2), 
                    new Card((Suite)1,(Rank)6), 
                    new Card((Suite)1,(Rank)8), 
                    new Card((Suite)1,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)3,(Rank)2),
                    new Card((Suite)3,(Rank)3), 
                    new Card((Suite)3,(Rank)7), 
                    new Card((Suite)3,(Rank)11), 
                    new Card((Suite)3,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)2,(Rank)14),
                    new Card((Suite)2,(Rank)9), 
                    new Card((Suite)2,(Rank)5), 
                    new Card((Suite)2,(Rank)2), 
                    new Card((Suite)2,(Rank)2)},
                new ICard[5] {
                    new Card((Suite)0,(Rank)2),
                    new Card((Suite)0,(Rank)3), 
                    new Card((Suite)0,(Rank)6), 
                    new Card((Suite)0,(Rank)6), 
                    new Card((Suite)0,(Rank)6)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)11),
                    new Card((Suite)1,(Rank)11), 
                    new Card((Suite)1,(Rank)13), 
                    new Card((Suite)1,(Rank)9), 
                    new Card((Suite)1,(Rank)13)},
            };
            int index = 0;
            foreach(Player player in game.Players)
            {
                player.DrawCards(HandList[index]);
                player.SortHand();
                player.IdentifyHand();
                index++;
            }
            //act
            game.HandleResult(game.Players.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, game.Players[0].Wins);
        }

        [Test]
                public void CanHandleResultsForWorseFlush()
        {
            for(var i = 0; i < 1000; i++)
            {
            Dealer dealer = new Dealer();
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();

            #region Create Best Player

            List<Rank> rankes = new List<Rank>();
                Suite suite = (Suite)TestMethods.RandomSuite();
                int pairs = 0;
                bool almostFullHouse = false;
                bool threeOfAKind = false;
                for(var j = 0; j < 5; j++)
                {
                    while(true)
                    {
                        bool isStraight = true;
                        Rank tempRank = (Rank)TestMethods.RandomRank();
                        if(rankes.Count == 4)
                        {
                            List<Rank> tempRankes = new List<Rank>();
                            foreach(Rank element in rankes)
                            {
                            tempRankes.Add(element);
                            }
                            tempRankes.Add(tempRank);
                            tempRankes.Sort();
                            for(var g = 0; g < 4; g++)
                            {
                                isStraight = (int)tempRankes[g] != (int)tempRankes[g + 1] - 1 ? false : isStraight; 
                            }
                            if((int)tempRankes[0] == 2 && (int)tempRankes[1] == 3 && (int)tempRankes[2] == 4 && 
                            (int)tempRankes[3] == 5 && (int)tempRankes[4] == 14)
                            {
                                isStraight = true;
                            }
                        }
                        Rank[] tempRankOccurrences = rankes.Where(element => element == tempRank).ToArray();
                        pairs += tempRankOccurrences.Length == 1 ? 1: 0;
                        threeOfAKind = tempRankOccurrences.Length == 2 ? true: threeOfAKind;
                        almostFullHouse = tempRankOccurrences.Length == 2 && pairs == 2 || 
                        tempRankOccurrences.Length == 1 && threeOfAKind ? true: almostFullHouse;
                        if(tempRankOccurrences.Length != 3 &&
                            !(almostFullHouse && rankes.Contains(tempRank)) &&
                            !(rankes.Count == 4 && isStraight) && 
                            (int)tempRank > 3)
                        {
                            rankes.Add(tempRank);
                            break;
                        }
                    }
                }
                var hand = new Card[5]
                    {new Card(suite, (Rank)rankes[0]), 
                    new Card(suite, (Rank)rankes[1]),
                    new Card(suite, (Rank)rankes[2]),
                    new Card(suite, (Rank)rankes[3]),
                    new Card(suite, (Rank)rankes[4])};
            Player bestPlayer = new Player("BestPlayer");
            bestPlayer.DrawCards(hand);
            bestPlayer.SortHand();
            bestPlayer.IdentifyHand();
            AllPlayers.Add(bestPlayer);
            #endregion
        
            #region Create Worse Players
            foreach(Player player in game.Players)
            {
                rankes = new List<Rank>();
                suite = (Suite)TestMethods.RandomSuite();
                pairs = 0;
                almostFullHouse = false;
                threeOfAKind = false;
                for(var j = 0; j < 5; j++)
                {
                    while(true)
                    {
                        bool isStraight = true;
                        Rank tempRank = (Rank)TestMethods.RandomRank();
                        if(rankes.Count == 4)
                        {
                            List<Rank> tempRankes = new List<Rank>();
                            foreach(Rank rank in rankes)
                            {
                                tempRankes.Add(rank);
                            }
                            tempRankes.Add(tempRank);
                            tempRankes.Sort();
                            for(var g = 0; g < 4; g++)
                            {
                                isStraight = (int)tempRankes[g] != (int)tempRankes[g + 1] - 1 ? false : isStraight; 
                            }
                            if((int)tempRankes[0] == 2 && (int)tempRankes[1] == 3 && (int)tempRankes[2] == 4 && 
                            (int)tempRankes[3] == 5 && (int)tempRankes[4] == 14)
                            {
                                isStraight = true;
                            }
                            if((int)tempRankes[0] == 3 && (int)tempRankes[1] == 4 && (int)tempRankes[2] == 5 && 
                            (int)tempRankes[3] == 6)
                            {
                                isStraight = true;
                            }
                        }
                        Rank[] tempRankOccurrences = rankes.Where(element => element == tempRank).ToArray();
                        pairs += tempRankOccurrences.Length == 1 ? 1: 0;
                        threeOfAKind = tempRankOccurrences.Length == 2 ? true: threeOfAKind;
                        almostFullHouse = tempRankOccurrences.Length == 2 && pairs == 2 || 
                        tempRankOccurrences.Length == 1 && threeOfAKind ? true: almostFullHouse;

                        if(tempRankOccurrences.Length < 3 &&
                            !(almostFullHouse && rankes.Contains(tempRank)) &&
                            !(rankes.Count == 4 && isStraight) &&
                            tempRank < bestPlayer.Hand[4].Rank)
                        {
                            rankes.Add(tempRank);
                            break;
                        }
                    }
                }

                hand = new Card[5]
                    {new Card(suite, (Rank)rankes[0]), 
                    new Card(suite, (Rank)rankes[1]),
                    new Card(suite, (Rank)rankes[2]),
                    new Card(suite, (Rank)rankes[3]),
                    new Card(suite, (Rank)rankes[4])};
                Player newPlayer = new Player("newPlayer");
                newPlayer.DrawCards(hand);
                newPlayer.SortHand();
                newPlayer.IdentifyHand();
                AllPlayers.Add(newPlayer);
            }
                #endregion
            
            //act
            game.HandleResult(AllPlayers.ToArray(), dealer);
            //assert
            Assert.AreEqual(1, bestPlayer.Wins);
            }
        }

        [Test]
        public void CanHandleResultsForWorseHandWithEqualStraights()
        {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            ICard[][] HandList = new ICard[5][] {
                new ICard[5] {
                    new Card((Suite)1,(Rank)10),
                    new Card((Suite)3,(Rank)11), 
                    new Card((Suite)0,(Rank)12), 
                    new Card((Suite)2,(Rank)13), 
                    new Card((Suite)1,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)2),
                    new Card((Suite)3,(Rank)3), 
                    new Card((Suite)0,(Rank)4), 
                    new Card((Suite)2,(Rank)5), 
                    new Card((Suite)1,(Rank)14)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)9),
                    new Card((Suite)3,(Rank)10), 
                    new Card((Suite)0,(Rank)11), 
                    new Card((Suite)2,(Rank)12), 
                    new Card((Suite)1,(Rank)13)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)2),
                    new Card((Suite)3,(Rank)3), 
                    new Card((Suite)0,(Rank)4), 
                    new Card((Suite)2,(Rank)5), 
                    new Card((Suite)1,(Rank)6)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)5),
                    new Card((Suite)3,(Rank)7), 
                    new Card((Suite)0,(Rank)8), 
                    new Card((Suite)2,(Rank)9), 
                    new Card((Suite)1,(Rank)10)},
            };
            int index = 0;
            foreach(Player player in game.Players)
            {
                player.DrawCards(HandList[index]);
                player.SortHand();
                player.IdentifyHand();
                index++;
            }
            //act
            game.HandleResult(game.Players.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, game.Players[0].Wins);
        }

        [Test]
                public void CanHandleResultsforWorseStraights()
        {
            for(var i = 0; i < 1000; i++)
            {
            Dealer dealer = new Dealer();
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();

            #region Create Best Player

            int firstSuiteOccurrences = 0; 
            Suite firstSuite = 0;
            List<Suite> suites = new List<Suite>();
            for(var j = 0; j < 5; j++)
            {
                while(true)
                {
                    Suite tempSuite = (Suite)TestMethods.RandomSuite();
                    firstSuite = j == 0 ? tempSuite : firstSuite;
                    firstSuiteOccurrences += tempSuite == firstSuite ? 1 : 0;
                    if(!(firstSuiteOccurrences >= 4 && tempSuite == firstSuite))
                    {
                        suites.Add(tempSuite);
                        break;
                    }
                }
            }
            int bestTempRank;
            do
            {
            bestTempRank = TestMethods.RandomRank();
            } while(bestTempRank == 1 || bestTempRank > 10);
            ICard[] hand = new ICard[5] 
            {new Card(suites[0], (Rank)bestTempRank), 
            new Card(suites[1], (Rank)bestTempRank + 1),
            new Card(suites[2], (Rank)bestTempRank + 2),
            new Card(suites[3], (Rank)bestTempRank + 3),
            new Card(suites[4], (Rank)bestTempRank + 4)};
            Player bestPlayer = new Player("BestPlayer");
            bestPlayer.DrawCards(hand);
            bestPlayer.SortHand();
            bestPlayer.IdentifyHand();
            AllPlayers.Add(bestPlayer);
            #endregion
        
            #region Create Worse Players
            foreach(Player player in game.Players)
            {
            firstSuiteOccurrences = 0; 
            firstSuite = 0;
            suites = new List<Suite>();
            for(var j = 0; j < 5; j++)
            {
                while(true)
                {
                    Suite tempSuite = (Suite)TestMethods.RandomSuite();
                    firstSuite = j == 0 ? tempSuite : firstSuite;
                    firstSuiteOccurrences += tempSuite == firstSuite ? 1 : 0;
                    if(!(firstSuiteOccurrences >= 4 && tempSuite == firstSuite))
                    {
                        suites.Add(tempSuite);
                        break;
                    }
                }
            }
            int tempRank;
            do
            {
            tempRank = TestMethods.RandomRank();
            } while(tempRank > bestTempRank);

            Player newPlayer = new Player("newPlayer");
                if(tempRank == bestTempRank)
                {
                    hand = new ICard[5] 
                    {new Card(suites[0], (Rank)2), 
                    new Card(suites[1], (Rank)3),
                    new Card(suites[2], (Rank)4),
                    new Card(suites[3], (Rank)5),
                    new Card(suites[4], (Rank)14)};
                    newPlayer.DrawCards(hand);
                }
                else
                {
                    hand = new ICard[5] 
                    {new Card(suites[0], (Rank)tempRank), 
                    new Card(suites[1], (Rank)tempRank + 1),
                    new Card(suites[2], (Rank)tempRank + 2),
                    new Card(suites[3], (Rank)tempRank + 3),
                    new Card(suites[4], (Rank)tempRank + 4)};
                    newPlayer.DrawCards(hand);
                }
                newPlayer.SortHand();
                newPlayer.IdentifyHand();
                AllPlayers.Add(newPlayer);
            }
                #endregion
            
            //act
            game.HandleResult(AllPlayers.ToArray(), dealer);
            //assert
            Assert.AreEqual(1, bestPlayer.Wins);
            }
        }

        [Test]
                public void CanHandleResultsForWorseHandsWithEqualThreeOfAKind()
        {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            ICard[][] HandList = new ICard[5][] {
                new ICard[5] {
                    new Card((Suite)1,(Rank)13),
                    new Card((Suite)3,(Rank)13), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)5), 
                    new Card((Suite)1,(Rank)9)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)13),
                    new Card((Suite)3,(Rank)13), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)2), 
                    new Card((Suite)1,(Rank)9)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)13),
                    new Card((Suite)3,(Rank)13), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)8), 
                    new Card((Suite)1,(Rank)7)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)13),
                    new Card((Suite)3,(Rank)13), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)2), 
                    new Card((Suite)1,(Rank)4)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)13),
                    new Card((Suite)3,(Rank)13), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)4), 
                    new Card((Suite)1,(Rank)9)},
            };
            int index = 0;
            foreach(Player player in game.Players)
            {
                player.DrawCards(HandList[index]);
                player.SortHand();
                player.IdentifyHand();
                index++;
            }
            //act
            game.HandleResult(game.Players.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, game.Players[0].Wins);
        }

        [Test]
                public void CanHandleResultsForWorseThreeOfAKind()
        {
            for(var i = 0; i < 10000; i++)
            {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();

            #region Create Best Player
            int bestRandomRank;
            do
            {
            bestRandomRank = TestMethods.RandomRank();
            } while (bestRandomRank == 2);
            var randomTOK = dealer.Deck.Where(card => (int)card.Rank == bestRandomRank).ToArray();
            Random random = new Random();
            while (randomTOK.Length > 3)
            {
                int removeCard = random.Next(0, randomTOK.Length);
                randomTOK[removeCard] = null;
                randomTOK = randomTOK.Where(card => card != null).ToArray();
            }
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != bestRandomRank).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player BestPlayer = new Player("Gustav");
            BestPlayer.DrawCards(randomTOK);
            BestPlayer.DrawCards(TestMethods.DealTopCards(2, smallTestDeck));
            BestPlayer.SortHand();
            BestPlayer.IdentifyHand();
            AllPlayers.Add(BestPlayer);
            #endregion
            
            #region Create Worse Players
            foreach(Player player in game.Players)
            {
                int randomRank;
                do
                {
                    randomRank = TestMethods.RandomRank();
                } while (randomRank >= bestRandomRank);
                randomTOK = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
                while (randomTOK.Length > 3)
                {
                    int removeCard = random.Next(0, randomTOK.Length);
                    randomTOK[removeCard] = null;
                    randomTOK = randomTOK.Where(card => card != null).ToArray();
                }
                smallTestDeck = TestMethods.OneOfEachRankDeck();
                smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
                smallTestDeck = TestMethods.Shuffle(smallTestDeck);
                Player newPlayer = new Player("Gustav");
                newPlayer.DrawCards(randomTOK);
                newPlayer.DrawCards(TestMethods.DealTopCards(2, smallTestDeck));
                newPlayer.SortHand();
                newPlayer.IdentifyHand();
                AllPlayers.Add(newPlayer);
                }
                #endregion
                //act
                game.HandleResult(AllPlayers.ToArray(), dealer);
                //assert
                Assert.AreEqual(1, BestPlayer.Wins);
            }
        }

        [Test]
                public void CanHandleResultsForWorseHandsWithEqualTwoPair()
        {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            ICard[][] HandList = new ICard[5][] {
                new ICard[5] {
                    new Card((Suite)1,(Rank)5),
                    new Card((Suite)3,(Rank)5), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)13), 
                    new Card((Suite)1,(Rank)9)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)5),
                    new Card((Suite)3,(Rank)5), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)13), 
                    new Card((Suite)1,(Rank)8)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)4),
                    new Card((Suite)3,(Rank)4), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)13), 
                    new Card((Suite)1,(Rank)2)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)5),
                    new Card((Suite)3,(Rank)5), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)13), 
                    new Card((Suite)1,(Rank)4)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)2),
                    new Card((Suite)3,(Rank)2), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)13), 
                    new Card((Suite)1,(Rank)7)},
            };
            int index = 0;
            foreach(Player player in game.Players)
            {
                player.DrawCards(HandList[index]);
                player.SortHand();
                player.IdentifyHand();
                index++;
            }
            //act
            game.HandleResult(game.Players.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, game.Players[0].Wins);
        }

        [Test]
        public void CanHandleResultsForWorseTwoPair()
        {
            for(var i = 0; i < 10000; i++)
            {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();

            #region Create Best Player
            dealer = new Dealer();
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for(var j = 0; j < 2; j++)
            {
                while(true)
                {
                    int tempNum = TestMethods.RandomRank();
                    if(!randomNumbers.Contains(tempNum) && tempNum > 3)
                    {
                        randomNumbers.Add(tempNum);
                        break;
                    }
                }
            }
            int BestPair = randomNumbers[0] > randomNumbers[1] ? randomNumbers[0]: randomNumbers[1];
            int randomRank1 = randomNumbers[0];
            int randomRank2 = randomNumbers[1];
            var randomPair1 = dealer.Deck.Where(card => (int)card.Rank == randomRank1).ToArray();
            var randomPair2 = dealer.Deck.Where(card => (int)card.Rank == randomRank2).ToArray();
            while (randomPair1.Length > 2)
            {
                int removeCard1 = random.Next(0, randomPair1.Length);
                int removeCard2 = random.Next(0, randomPair2.Length);
                randomPair1[removeCard1] = null;
                randomPair2[removeCard2] = null;
                randomPair1 = randomPair1.Where(card => card != null).ToArray();
                randomPair2 = randomPair2.Where(card => card != null).ToArray();
            }
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank1).ToArray();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank2).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player BestPlayer = new Player("Gustav");
            BestPlayer.DrawCards(randomPair1);
            BestPlayer.DrawCards(randomPair2);
            BestPlayer.DrawCards(TestMethods.DealTopCards(1, smallTestDeck));
            BestPlayer.SortHand();
            BestPlayer.IdentifyHand();
            AllPlayers.Add(BestPlayer);
            #endregion

            #region Create Worse Players
            foreach(Player player in game.Players)
            {
            dealer = new Dealer();
            randomNumbers = new List<int>();
            for(var j = 0; j < 2; j++)
            {
                while(true)
                {
                    int tempNum = TestMethods.RandomRank();
                    if(!randomNumbers.Contains(tempNum) && tempNum < BestPair)
                    {
                        randomNumbers.Add(tempNum);
                        break;
                    }
                }
            }
            randomRank1 = randomNumbers[0];
            randomRank2 = randomNumbers[1];
            randomPair1 = dealer.Deck.Where(card => (int)card.Rank == randomRank1).ToArray();
            randomPair2 = dealer.Deck.Where(card => (int)card.Rank == randomRank2).ToArray();
            while (randomPair1.Length > 2)
            {
                int removeCard1 = random.Next(0, randomPair1.Length);
                int removeCard2 = random.Next(0, randomPair2.Length);
                randomPair1[removeCard1] = null;
                randomPair2[removeCard2] = null;
                randomPair1 = randomPair1.Where(card => card != null).ToArray();
                randomPair2 = randomPair2.Where(card => card != null).ToArray();
            }
            smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank1).ToArray();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank2).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player newPlayer = new Player("Gustav");
            newPlayer.DrawCards(randomPair1);
            newPlayer.DrawCards(randomPair2);
            newPlayer.DrawCards(TestMethods.DealTopCards(1, smallTestDeck));
            newPlayer.SortHand();
            newPlayer.IdentifyHand();
            AllPlayers.Add(newPlayer);
            }
            #endregion
            //act
            game.HandleResult(AllPlayers.ToArray(), dealer);
            //assert
            Assert.AreEqual(1, BestPlayer.Wins);
            }
        }

        [Test]
        public void CanHandleResultsForWorseHandsWithEqualPairs()
        {
            //assemble
            Dealer dealer = new Dealer();
            string[] playernames = new string[5] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",
                "player5"};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            ICard[][] HandList = new ICard[5][] {
                new ICard[5] {
                    new Card((Suite)1,(Rank)11),
                    new Card((Suite)3,(Rank)11), 
                    new Card((Suite)0,(Rank)14), 
                    new Card((Suite)2,(Rank)10), 
                    new Card((Suite)1,(Rank)4)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)11),
                    new Card((Suite)3,(Rank)11), 
                    new Card((Suite)0,(Rank)14), 
                    new Card((Suite)2,(Rank)10), 
                    new Card((Suite)1,(Rank)2)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)11),
                    new Card((Suite)3,(Rank)11), 
                    new Card((Suite)0,(Rank)14), 
                    new Card((Suite)2,(Rank)9), 
                    new Card((Suite)1,(Rank)8)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)11),
                    new Card((Suite)3,(Rank)11), 
                    new Card((Suite)0,(Rank)13), 
                    new Card((Suite)2,(Rank)10), 
                    new Card((Suite)1,(Rank)9)},
                new ICard[5] {
                    new Card((Suite)1,(Rank)11),
                    new Card((Suite)3,(Rank)11), 
                    new Card((Suite)0,(Rank)10), 
                    new Card((Suite)2,(Rank)4), 
                    new Card((Suite)1,(Rank)2)},
            };
            int index = 0;
            foreach(Player player in game.Players)
            {
                player.DrawCards(HandList[index]);
                player.SortHand();
                player.IdentifyHand();
                index++;
            }
            //act
            game.HandleResult(game.Players.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, game.Players[0].Wins);
        }

        [Test]
        public void CanHandleResultsForWorsePairs()
        {
            for(var i = 0; i < 10000; i++)
            {
            //assemble
            Dealer dealer = new Dealer();
            Player BestPlayer = new Player("BestPlayer");
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();

            #region Create BestPlayer

            int randomRank;
            do
            {
            randomRank = TestMethods.RandomRank();
            } while (randomRank == 2);
            var bestRandomPair = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            Random random = new Random();
            while (bestRandomPair.Length > 2)
            {
                int removeCard = random.Next(0, bestRandomPair.Length);
                bestRandomPair[removeCard] = null;
                bestRandomPair = bestRandomPair.Where(card => card != null).ToArray();
            }
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            BestPlayer.DrawCards(TestMethods.DealTopCards(3, smallTestDeck));
            var bestRankes = BestPlayer.Hand.Select(card => card.Rank).ToList(); 
            bestRankes.Sort();
            BestPlayer.DrawCards(bestRandomPair);
            BestPlayer.SortHand();
            BestPlayer.IdentifyHand();
            AllPlayers.Add(BestPlayer);
            Rank[] BestPlayerRanks = BestPlayer.Hand.Select(element => element.Rank).ToArray();
            #endregion

            #region Create Players With Worse Pairs
            foreach(Player player in game.Players)
            {
            dealer = new Dealer();
            do
            {
            randomRank = TestMethods.RandomRank();
            } while(randomRank >= (int)bestRandomPair[0].Rank);
            var randomPair = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            random = new Random();
            while (randomPair.Length > 2)
            {
                int removeCard = random.Next(0, randomPair.Length);
                randomPair[removeCard] = null;
                randomPair = randomPair.Where(card => card != null).ToArray();
            }
            smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player newPlayer = new Player("Gustav");
            //newPlayer.DrawCards(randomPair);
            newPlayer.DrawCards(TestMethods.DealTopCards(3, smallTestDeck));
            newPlayer.DrawCards(randomPair);
            newPlayer.SortHand();
            newPlayer.IdentifyHand();
            AllPlayers.Add(newPlayer);
            }
            #endregion
            //act
            game.HandleResult(AllPlayers.ToArray(), dealer);
            //assert
                Assert.AreEqual(1, BestPlayer.Wins);
            }
        }

        [Test]
        public void CanHandleResultsForHighCard()
        {
            for(var i = 0; i < 10000; i++)
            {
            //assemble
            Dealer dealer = new Dealer();
            Player BestPlayer = new Player("BestPlayer");
            string[] playernames = new string[4] {
                "Player1",
                "Player2", 
                "Player3", 
                "Player4",};
            PokerGame game = new PokerGame(playernames);
            game.Winner += TestMethods.OnWinner;
            game.Draw += TestMethods.OnDraw;
            List<ICard[]> handList = new List<ICard[]>();
            List<IPlayer> AllPlayers = new List<IPlayer>();
            bool isDraw = false;

            #region Create BestPlayer

            int firstSuiteOccurrences = 0; 
            Suite firstSuite = 0;
            List<Suite> suites = new List<Suite>();
            List<Rank> rankes = new List<Rank>();
            
            for(var j = 0; j < 5; j++)
            {
                while(true)
                {
                    Suite tempSuite = (Suite)TestMethods.RandomSuite();
                    firstSuite = j == 0 ? tempSuite : firstSuite;
                    firstSuiteOccurrences += tempSuite == firstSuite ? 1 : 0;
                    if(!(firstSuiteOccurrences >= 5 && tempSuite == firstSuite))
                    {
                        suites.Add(tempSuite);
                        break;
                    }
                }
                while(true)
                {
                    bool isStraight = true;
                    Rank tempRank = (Rank)TestMethods.RandomRank();
                    if(rankes.Count == 4)
                    {
                        List<Rank> tempRankes = new List<Rank>();
                        foreach(Rank element in rankes)
                        {
                        tempRankes.Add(element);
                        }
                        tempRankes.Add(tempRank);
                        tempRankes.Sort();
                        for(var g = 0; g < 4; g++)
                        {
                            isStraight = (int)tempRankes[g] != (int)tempRankes[g + 1] - 1 ? false : isStraight; 
                        }
                        if((int)tempRankes[0] == 2 && (int)tempRankes[1] == 3 && (int)tempRankes[2] == 4 && 
                        (int)tempRankes[3] == 5 && (int)tempRankes[4] == 14)
                        {
                            isStraight = true;
                        }
                    }
                    if(!rankes.Contains(tempRank) && !(rankes.Count == 4 && isStraight))
                    {
                        rankes.Add(tempRank);
                        break;
                    }
                }
            }
            ICard[] bestPlayerHand = new ICard[5] 
                {new Card(suites[0], rankes[0]), 
                new Card(suites[1], rankes[1]),
                new Card(suites[2], rankes[2]),
                new Card(suites[3], rankes[3]),
                new Card(suites[4], rankes[4])};
            BestPlayer.DrawCards(bestPlayerHand);
            BestPlayer.SortHand();
            AllPlayers.Add(BestPlayer);
            Rank[] BestPlayerRanks = BestPlayer.Hand.Select(element => element.Rank).ToArray();
            #endregion

            #region Create Equal or Worse Players
            foreach(Player player in game.Players)
            {
            firstSuiteOccurrences = 0; 
            firstSuite = 0;
            suites = new List<Suite>();
            rankes = new List<Rank>();
            
            for(var j = 0; j < 5; j++)
            {
                while(true)
                {
                    //Randomizes Suites and makes sure a Flush doesn't happen
                    Suite tempSuite = (Suite)TestMethods.RandomSuite();
                    firstSuite = j == 0 ? tempSuite : firstSuite;
                    firstSuiteOccurrences += tempSuite == firstSuite ? 1 : 0;
                    if(!(firstSuiteOccurrences >= 5 && tempSuite == firstSuite))
                    {
                        suites.Add(tempSuite);
                        break;
                    }
                }
                while(true)
                {
                    bool isStraight = true;
                    Rank tempRank = (Rank)TestMethods.RandomRank();
                    //Makes sure a straight doesn't occur
                    if(rankes.Count == 4)
                    {
                        List<Rank> tempRankes = new List<Rank>();
                        foreach(Rank rank in rankes)
                        {
                            tempRankes.Add(rank);
                        }
                        tempRankes.Add(tempRank);
                        tempRankes.Sort();
                        for(var g = 0; g < 4; g++)
                        {
                            isStraight = (int)tempRankes[g] != (int)tempRankes[g + 1] - 1 ? false : isStraight; 
                        }
                        if((int)tempRankes[0] == 2 && (int)tempRankes[1] == 3 && (int)tempRankes[2] == 4 && 
                        (int)tempRankes[3] == 5 && (int)tempRankes[4] == 14)
                        {
                            isStraight = true;
                        }
                        if((int)tempRankes[0] == 3 && (int)tempRankes[1] == 4 && (int)tempRankes[2] == 5 && 
                        (int)tempRankes[3] == 6)
                        {
                            rankes[3] = BestPlayer.Hand[4].Rank;
                        }
                    }
                    if(!rankes.Contains(tempRank) && !(rankes.Count == 4 && isStraight) && tempRank <= BestPlayer.Hand[4].Rank)
                    {
                        //adds ranomized rank for the future creation of the players hand
                        rankes.Add(tempRank);
                        break;
                    }
                }
            }
            rankes.Sort();
            //Makes sure the hand won't become larger than the expected best hand
            if(rankes.Contains(BestPlayer.Hand[4].Rank))
            {
                Random random = new Random();
                for(var l = 3; l >= 0; l--)
                {
                    rankes.Sort();
                    for(var m = l; m >= 0; m--)
                    {
                        //Makes sure rankes in same and lower positions as BestPlayersHands Rank for positon L(l) aren't larger than it.
                        if(rankes[m] > BestPlayer.Hand[l].Rank)
                        {   
                            while(true)
                            {
                                bool isStraight = true;
                                //Randomizes new rank same or lower than BestPlayersHands Rank for positon L(l)
                                Rank tempRank = (Rank)random.Next(2, (int)BestPlayer.Hand[l].Rank + 1); 
                                if(l == 0)
                                {
                                    List<Rank> tempRankes = new List<Rank>();
                                    for(var j = 1; j < rankes.Count; j++)
                                    {
                                    tempRankes.Add(rankes[j]);
                                    }
                                    tempRankes.Add(tempRank);
                                    tempRankes.Sort();
                                    for(var g = 0; g < 4; g++)
                                    {
                                        isStraight = (int)tempRankes[g] != (int)tempRankes[g + 1] - 1 ? false : isStraight; 
                                    }
                                    if((int)tempRankes[0] == 2 && (int)tempRankes[1] == 3 && (int)tempRankes[2] == 4 && 
                                    (int)tempRankes[3] == 5 && (int)tempRankes[4] == 14)
                                    {
                                        isStraight = true;
                                    }
                                }
                                if(!rankes.Contains(tempRank) && !(l == 0 && isStraight))
                                {
                                    rankes[m] = tempRank;
                                    break;
                                }
                            }
                        }
                    }
                    bool isWorse = true;
                    for(var n = l; n >= 0; n--)
                    {
                        isWorse = rankes[n] >= BestPlayer.Hand[l].Rank ? false: isWorse;
                    }
                    if(isWorse)
                    {
                        break;
                    }
                }
            }
        
            ICard[] hand = new ICard[5] 
                {new Card(suites[0], rankes[0]), 
                new Card(suites[1], rankes[1]),
                new Card(suites[2], rankes[2]),
                new Card(suites[3], rankes[3]),
                new Card(suites[4], rankes[4])};
                handList.Add(hand);
            }

            int index = 0;
            foreach (Player player in game.Players)
            {
                player.DrawCards(handList[index]);
                player.SortHand();
                AllPlayers.Add(player);
                index++;
            }
            #endregion
            //act
            foreach(Player player in game.Players)
            {
                Rank[] playerRanks = player.Hand.Select(element => element.Rank).ToArray();
                foreach(Rank rank in BestPlayerRanks)
                {
                    if(!playerRanks.Contains(rank))
                    {
                        isDraw = false;
                        break;
                    }
                    else
                    {
                        isDraw = true;
                    }
                }
                if(isDraw)
                {
                    break;
                }
            }
            game.HandleResult(AllPlayers.ToArray(), dealer);
            //assert
            if(isDraw)
            {
                foreach(Player player in AllPlayers)
                {
                    Assert.AreEqual(0, player.Wins);
                }
            }
            else
            {
                Assert.AreEqual(1, BestPlayer.Wins);
            }
            }
        }
    }
}