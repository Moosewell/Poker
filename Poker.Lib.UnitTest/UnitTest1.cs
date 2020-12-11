using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;



namespace Poker.Lib.UnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void NewPlayerCanBeInstanced()
        {
            string playerName = "Gustav";
            Player player = new Player(playerName);
            CollectionAssert.AreEqual(player.Hand, new Card[] {null, null, null, null, null});
            CollectionAssert.AreEqual(player.Discard, new Card[] {null, null, null, null, null});
            Assert.AreEqual(player.Name, playerName);
            Assert.AreEqual(player.Wins, 0);
        }

        [Test]
        public void CardsCanBeAddedToHand()
        {
            var cards = new Card[5];
            cards = ToCards("♥10♥J♥Q♥K♥A");
            Player player = new Player("Gustav");
            player.DrawCards(cards);
            for (int i = 0; i < 5; ++i) 
            {
                var takeData = cards.Take(i + 1);
                CollectionAssert.AreEqual(cards.Take(i + 1), player.Hand.Take(i + 1));
            }
        }

        [Test]
        public void MaximumFiveCardsInHand()
        {
            var cards = new Card[6];
            cards = ToCards("♥10♦J♥Q♥K♣A♣4");
            Player player = new Player("Gustav");
            CollectionAssert.AreEqual(player.Hand, new Card[] {null, null, null, null, null});
            Assert.Throws<IndexOutOfRangeException>(() => player.DrawCards(cards));
            
        }

        [Test]
        public void CardsCanBeDiscarded()
        {
            //assemble
            var cards = new Card[5];
            cards = ToCards("♥10♥J♥Q♥K♥A");
            Player player = new Player("Gustav");
            player.DrawCards(cards);
            var cardsToDiscard = ToCards("♥10♥K♥A");
            player.Discard = cardsToDiscard;
            //act
            player.DiscardCards();
            //assert
            for(var i = 0; i < cardsToDiscard.Length; i++)
            {
            CollectionAssert.DoesNotContain(player.Hand, cardsToDiscard[i]);
            }
        }

        [Test]
        public void CanSortHand()
        {
            //assemble
            var cards = new Card[5];
            cards = ToCards("♣10♠J♥Q♦K♦10");
            Player player = new Player("Gustav");
            player.DrawCards(cards);
            //act
            player.SortHand();
            //assert
            for(var i = 0; i <player.Hand.Length - 1; i++)
            {
                Assert.LessOrEqual(player.Hand[i].Rank, player.Hand[i + 1].Rank);
                if(player.Hand[i].Rank == player.Hand[i + 1].Rank)
                {
                    Assert.Less(player.Hand[i].Suite, player.Hand[i + 1].Suite);
                }
            }
        }

        [Test]
        public void CanClearPiles()
        {
            //assemble
            var cards = new Card[5];
            cards = ToCards("♣10♠J♥Q♦K♦10");
            Player player = new Player("Gustav");
            player.DrawCards(cards);
            player.Discard = ToCards("♣K♠2♥3");
            //act
            player.ClearPiles();
            //assert
            CollectionAssert.AreEqual(player.Hand, new Card[] {null, null, null, null, null});
            CollectionAssert.AreEqual(player.Discard, new Card[] {null, null, null, null, null});
        }

        [Test]
        public void CanAssignRoyalStraightFlush()
        {
            for(var i = 0; i < 100; i++)
            {
            //assemble
            Suite suite = (Suite)RandomSuite();
            var cards = new Card[5]
                    {new Card(suite, (Rank)10), 
                    new Card(suite, (Rank)11),
                    new Card(suite, (Rank)12),
                    new Card(suite, (Rank)13),
                    new Card(suite, (Rank)14)};
            Player player = new Player("Gustav");
            player.DrawCards(cards);
            player.SortHand();
            //act
            player.IdentifyHand();
            //assert
            Assert.IsTrue(player.HandType == HandType.RoyalStraightFlush);
            }
        }

        [Test]
        public void CanAssignStraightFlush()
        {
            for(var i = 0; i < 100; i++)
            {
            //assemble
            Suite suite = (Suite)RandomSuite();
            for(var g = 1; g < 10; g++)
            {
                Player player = new Player("Gustav");
                if(g == 1)
                {
                    ICard[] hand = new ICard[5] 
                    {new Card(suite, (Rank)2), 
                    new Card(suite, (Rank)3),
                    new Card(suite, (Rank)4),
                    new Card(suite, (Rank)5),
                    new Card(suite, (Rank)14)};
                    player.DrawCards(hand);
                }
                else
                {
                    ICard[] hand = new ICard[5] 
                    {new Card(suite, (Rank)g), 
                    new Card(suite, (Rank)g + 1),
                    new Card(suite, (Rank)g + 2),
                    new Card(suite, (Rank)g + 3),
                    new Card(suite, (Rank)g + 4)};
                    player.DrawCards(hand);
                    player.SortHand();
                }
            
            //act
            player.IdentifyHand();
            //assert
            Assert.IsTrue(player.HandType == HandType.StraightFlush);
            }
            }
        }

        [Test]
        public void CanAssignFourOfAKind()
        {
            for(var i = 0; i < 100; i++)
            {
            Dealer dealer = new Dealer();
            int randomRank = RandomRank();
            var randomFOK = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            var smallTestDeck = OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomFOK);
            player.DrawCards(DealTopCards(1, smallTestDeck));
            player.SortHand();
            //act
            player.IdentifyHand();
            //assert
            Assert.IsTrue(player.HandType == HandType.FourOfAKind);
            }
        }
    
        [Test]
        public void CanAssignFullHouse()
        {
            for(var i = 0; i < 100; i++)
            {
            Dealer dealer = new Dealer();
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for(var j = 0; j < 2; j++)
            {
                while(true)
                {
                    int tempNum = RandomRank();
                    if(!randomNumbers.Contains(tempNum))
                    {
                        randomNumbers.Add(tempNum);
                        break;
                    }
                }
            }
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
            var smallTestDeck = OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank1).ToArray();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank2).ToArray();
            smallTestDeck = Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomPair1);
            player.DrawCards(randomPair2);
            player.SortHand();
            //act
            player.IdentifyHand();
            //assert
            Assert.IsTrue(player.HandType == HandType.FullHouse);
            }
        }

        [Test]
        public void CanAssignFlush()
        {
            for(var i = 0; i < 100; i++)
            {   
                //assemble
                List<Rank> rankes = new List<Rank>();
                bool isStraight = true;
                Suite suite = (Suite)RandomSuite();
                int pairs = 0;
                bool almostFullHouse = false;
                bool threeOfAKind = false;
                for(var j = 0; j < 5; j++)
                {
                    while(true)
                    {
                        Rank tempRank = (Rank)RandomRank();
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
                            !(rankes.Count == 4 && isStraight))
                        {
                            rankes.Add(tempRank);
                            break;
                        }
                    }
                }
                var cards = new Card[5]
                    {new Card(suite, (Rank)rankes[0]), 
                    new Card(suite, (Rank)rankes[1]),
                    new Card(suite, (Rank)rankes[2]),
                    new Card(suite, (Rank)rankes[3]),
                    new Card(suite, (Rank)rankes[4])};
                Player player = new Player("Gustav");
                player.DrawCards(cards);
                player.SortHand();
                //act
                player.IdentifyHand();
                //assert
                Assert.IsTrue(player.HandType == HandType.Flush);
            }
        }

        [Test]
        public void CanAssignStraight()
        {
            for(var i = 0; i < 100; i++)
            {
            //assemble
            int firstSuiteOccurrences = 0; 
            Suite firstSuite = 0;
            List<Suite> suites = new List<Suite>();
            for(var j = 0; j < 5; j++)
            {
                while(true)
                {
                    Suite tempSuite = (Suite)RandomSuite();
                    firstSuite = j == 0 ? tempSuite : firstSuite;
                    firstSuiteOccurrences += tempSuite == firstSuite ? 1 : 0;
                    if(!(firstSuiteOccurrences == 4 && tempSuite == firstSuite))
                    {
                        suites.Add(tempSuite);
                        break;
                    }
                }
            }
            for(var g = 1; g < 11; g++)
            {
                Player player = new Player("Gustav");
                if(g == 1)
                {
                    ICard[] hand = new ICard[5] 
                    {new Card(suites[0], (Rank)2), 
                    new Card(suites[1], (Rank)3),
                    new Card(suites[2], (Rank)4),
                    new Card(suites[3], (Rank)5),
                    new Card(suites[4], (Rank)14)};
                    player.DrawCards(hand);
                }
                else
                {
                    ICard[] hand = new ICard[5] 
                    {new Card(suites[0], (Rank)g), 
                    new Card(suites[1], (Rank)g + 1),
                    new Card(suites[2], (Rank)g + 2),
                    new Card(suites[3], (Rank)g + 3),
                    new Card(suites[4], (Rank)g + 4)};
                    player.DrawCards(hand);
                    player.SortHand();
                }
                //act
                player.IdentifyHand();
                //assert
                Assert.IsTrue(player.HandType == HandType.Straight);
            }
            }
        }

        [Test]
        public void CanAssignThreeOfAKind()
        {
            for(var i = 0; i < 100; i++)
            {
            Dealer dealer = new Dealer();
            int randomRank = RandomRank();
            var randomTOK = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            Random random = new Random();
            while (randomTOK.Length > 3)
            {
                int removeCard = random.Next(0, randomTOK.Length);
                randomTOK[removeCard] = null;
                randomTOK = randomTOK.Where(card => card != null).ToArray();
            }
            var smallTestDeck = OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomTOK);
            player.DrawCards(DealTopCards(2, smallTestDeck));
            player.SortHand();
            //act
            player.IdentifyHand();
            //assert
            Assert.IsTrue(player.HandType == HandType.ThreeOfAKind);
            }
        }

        [Test]
        public void CanAssignTwoPairs()
        {
            for(var i = 0; i < 100; i++)
            {
            Dealer dealer = new Dealer();
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for(var j = 0; j < 2; j++)
            {
                while(true)
                {
                    int tempNum = RandomRank();
                    if(!randomNumbers.Contains(tempNum))
                    {
                        randomNumbers.Add(tempNum);
                        break;
                    }
                }
            }
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
            var smallTestDeck = OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank1).ToArray();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank2).ToArray();
            smallTestDeck = Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomPair1);
            player.DrawCards(randomPair2);
            player.DrawCards(DealTopCards(1, smallTestDeck));
            player.SortHand();
            //act
            player.IdentifyHand();
            //assert
            Assert.IsTrue(player.HandType == HandType.TwoPairs);
            }
        }

        [Test]
        public void CanAssignPairs()
        {
            //assemble
            for(var i = 0; i < 100; i++)
            {
            Dealer dealer = new Dealer();
            int randomRank = RandomRank();
            var randomPair = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            Random random = new Random();
            while (randomPair.Length > 2)
            {
                int removeCard = random.Next(0, randomPair.Length);
                randomPair[removeCard] = null;
                randomPair = randomPair.Where(card => card != null).ToArray();
            }
            var smallTestDeck = OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomPair);
            player.DrawCards(DealTopCards(3, smallTestDeck));
            player.SortHand();
            //act
            player.IdentifyHand();
            //assert
            Assert.IsTrue(player.HandType == HandType.Pair);
            }
        }

        [Test]
        public void CanAssignHighCard()
        {
            for (var i = 0; i < 10000; i++)
            {
                //assemble
                int firstSuiteOccurrences = 0; 
                Suite firstSuite = 0;
                List<Suite> suites = new List<Suite>();
                List<Rank> rankes = new List<Rank>();
                for(var j = 0; j < 5; j++)
                {
                    while(true)
                    {
                        Suite tempSuite = (Suite)RandomSuite();
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
                        Rank tempRank = (Rank)RandomRank();
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
                Player player = new Player("Gustav");
                ICard[] hand = new ICard[5] 
                {new Card(suites[0], rankes[0]), 
                new Card(suites[1], rankes[1]),
                new Card(suites[2], rankes[2]),
                new Card(suites[3], rankes[3]),
                new Card(suites[4], rankes[4])};
                player.DrawCards(hand);
                player.SortHand();
                //act
                player.IdentifyHand();
                //assert
                Assert.IsTrue(player.HandType == HandType.HighCard);
            }
        }

        [Test]
        public void NewDealerCanBeInstanced()
        {
            //Assemble
            //Act
            Dealer dealer = new Dealer();
            //Assert
            Assert.AreEqual(52, dealer.Deck.Length);
            CollectionAssert.AllItemsAreUnique(dealer.Deck);
        }

        [Test]
        public void CanShuffleDeck()
        {
            //Assemble
            int iterations = 100;
            Dealer[] dealers = new Dealer[iterations];
            for(var i = 0; i < iterations; i++)
            {
                dealers[i] = new Dealer();
            }
            //Act
            foreach(Dealer dealer in dealers)
            {
            dealer.Shuffle();
            }
            //Assert
            CollectionAssert.AllItemsAreUnique(dealers);
        }

        [Test]
        public void CanRestoreDeck()
        {
            //assemble
            Dealer dealer = new Dealer();
            dealer.Shuffle();
            Player[] players = new Player[5] {
                new Player("Gustav"), 
                new Player("Gustav"), 
                new Player("Gustav"), 
                new Player("Gustav"), 
                new Player("Gustav")};
            foreach(Player player in players)
            {
                player.DrawCards(dealer.DealTopCards(5));
            }
            //act
            dealer.Restore(players);
            //assert
            Assert.AreEqual(52, dealer.Deck.Length);
            CollectionAssert.AllItemsAreUnique(dealer.Deck);
            foreach(Player player in players)
            {
                CollectionAssert.AreEqual(player.Hand, new Card[] {null, null, null, null, null});
                CollectionAssert.AreEqual(player.Discard, new Card[] {null, null, null, null, null});
            }

        }

        [Test]
        public void CanDealHands()
        {
            //assemble
            Dealer dealer = new Dealer();
            dealer.Shuffle();
            Player[] players = new Player[5] {
                new Player("Gustav"), 
                new Player("Gustav"), 
                new Player("Gustav"), 
                new Player("Gustav"), 
                new Player("Gustav")};
            ICard[] distrubutedCards = new ICard[5 * players.Length];
            for(var i = 0; i < distrubutedCards.Length; i++)
            {
                distrubutedCards[i] = dealer.Deck[dealer.Deck.Length - 1 - i]; 
            }
            //act
            dealer.DealHands(players);
            //assert
            Assert.AreEqual(52 - 5 * players.Length, dealer.Deck.Length);
            for(var i = 0; i < players.Length; i++)
            {
                CollectionAssert.DoesNotContain(players[i].Hand, null);
                Assert.AreEqual(5, players[i].Hand.Length);
                for(var j = 0; j < 5; j++)
                {
                    CollectionAssert.Contains(players[i].Hand, distrubutedCards[i + (j * 5)]);
                }
            }
        }

        [Test]
        public void CanDealTopCards()
        {
            //assemble
            Dealer dealer = new Dealer();
            dealer.Shuffle();
            ICard[] copiedDeck = dealer.Deck;
            ICard[] oneCardDrawn = new ICard[1];
            ICard[] twoCardsDrawn = new ICard[2];
            ICard[] threeCardsDrawn = new ICard[3];
            ICard[] fourCardsDrawn = new ICard[4];
            ICard[] fiveCardsDrawn = new ICard[5];
            ICard[] sixteenCardsDrawn = new ICard[16];
            ICard[][] arrayContainer = new ICard[][] {
                oneCardDrawn,
                twoCardsDrawn, 
                threeCardsDrawn, 
                fourCardsDrawn, 
                fiveCardsDrawn, 
                sixteenCardsDrawn};
            //act
            for(var i = 0; i < arrayContainer.Length; i++)
            {
                arrayContainer[i] = i != 5 ? dealer.DealTopCards(i + 1): dealer.DealTopCards(16);
            }
            //assert
            Assert.AreEqual(52 - 31, dealer.Deck.Length);
            for(var i = 0; i < 52 - 31; i++)
            {
                Assert.AreEqual(copiedDeck[i], dealer.Deck[i]);
            }
            foreach(ICard[] array in arrayContainer)
            {
                CollectionAssert.DoesNotContain(array, null);
            }
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

        //[Test]
        public void CanSaveAndExitGame()
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
            //game.SaveGameAndExit()
            //assert
        }

        [Test]
        public void CanLoadOldGame()
        {
            //assemble
            //act
            //assert
        }

        [Test]
        public void CanRunGame()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
        public void CanHandleResultsForRoyalStraightFlush()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
        public void CanHandleResultsForStraightFlush()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
                public void CanHandleResultsForFourOfAKind()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
                public void CanHandleResultsForFullHouse()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
                public void CanHandleResultsForFlush()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
                public void CanHandleResultsforStraight()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
                public void CanHandleResultsForThreeOfAKind()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
        public void CanHandleResultsForTwoPair()
        {
            //assemble
            //act
            //assert
        }

        [Test, Combinatorial]
        public void CanHandleResultsForPair()
        {
            //assemble
            //act
            //assert
        }

        [Test]
        public void CanHandleResultsForHighCard()
        {
            for(var i = 0; i < 1000; i++)
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
            game.Winner += OnWinner;
            game.Draw += OnDraw;
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
                    Suite tempSuite = (Suite)RandomSuite();
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
                    Rank tempRank = (Rank)RandomRank();
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

            #region Create Worse Players
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
                    Suite tempSuite = (Suite)RandomSuite();
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
                    Rank tempRank = (Rank)RandomRank();
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
                    if(!rankes.Contains(tempRank) && !(rankes.Count == 4 && isStraight) && tempRank <= BestPlayer.Hand[4].Rank)
                    {
                        rankes.Add(tempRank);
                        break;
                    }
                }
            }
            rankes.Sort();
            if(rankes.Contains(BestPlayer.Hand[4].Rank))
            {
                Random random = new Random();
                for(var l = 3; l >= 0; l--)
                {
                    rankes.Sort();
                    for(var m = l; m >= 0; m--)
                    {
                        if(rankes[m] > BestPlayer.Hand[l].Rank)
                        {   
                            while(true)
                            {
                                bool isStraight = true;
                                Rank tempRank = (Rank)random.Next(2, (int)BestPlayer.Hand[l].Rank + 1);
                                if(l == 0)
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


//////////////////////////////////////////////////////////////////////////////////////////
    public int RandomSuite()
    {
        Random random = new Random();
        return random.Next(0, 4);
    }

    public int RandomRank()
    {
        Random random = new Random();
        return random.Next(2, 15);
    }

    public ICard[] OneOfEachRankDeck()
    {
        var Deck = new ICard[13];
            int deckPosition = 0;
                    for(var rank = 2; rank < 15; rank++)
                    {
                        Deck[deckPosition] = new Card((Suite)RandomSuite(), (Rank)rank);
                        deckPosition++;
                    }
        return Deck;
    }

    public ICard[] Shuffle(ICard[] inputDeck)
        {
            ICard[] shuffledDeck = new ICard[inputDeck.Length];
            Random RandomNumber = new Random();
            List<int> occupiedPositions = new List<int>();
            foreach (ICard Card in inputDeck)
            {
                bool SearchAvailablePos = true;
                do
                {
                    int randomPosition = RandomNumber.Next(0, inputDeck.Length);
                    if(!occupiedPositions.Contains(randomPosition))
                    {
                        shuffledDeck[randomPosition] = Card;
                        occupiedPositions.Add(randomPosition);
                        SearchAvailablePos = false;
                    }
                } while(SearchAvailablePos);
            }
            inputDeck = shuffledDeck;
            return inputDeck;
        }

        public ICard[] DealTopCards(int amountToDraw, ICard[] inputDeck)
        {
            List<ICard> Cards = new List<ICard>();
            for(var Drawn = 0; Drawn < amountToDraw; Drawn++)
            {
                Cards.Add(inputDeck[inputDeck.Length - 1]);
                inputDeck[inputDeck.Length - 1] = null;
                inputDeck = inputDeck.Where(Card => Card != null).ToArray();
            }
            return Cards.ToArray();
        }

    static Card[] ToCards(string text) {
    List<Card> cards = new List<Card>();
    int i = 0; 
    while(i < text.Length) {
        Suite suite = (text[i]) switch {
            '♣' => Suite.Clubs, '♦' => Suite.Diamonds, '♥' => Suite.Hearts, '♠'=> Suite.Spades, 
            _ => throw new NotImplementedException(),
        };
        var rankString = text.Substring(i + 1);
        var rankFunc = new Dictionary<string, Func<string, Rank>>() {
            {@"^J",  _ => Rank.Jack}, {@"^Q", _ => Rank.Queen}, {@"^K", _ => Rank.King}, 
            {@"^A", _ => Rank.Ace}, { @"^\d+", str => (Rank)int.Parse(str) }
        };
        var func = rankFunc.Where(func => Regex.IsMatch(rankString, func.Key)).First();
        cards.Add(new Card(suite, func.Value(Regex.Match(rankString, func.Key).Value)));
        i += Regex.IsMatch(rankString, @"^\d\d") ? 3 : 2;
        }
    return cards.ToArray();
    }
            private void OnWinner(IPlayer player)
        {
        }

        private void OnDraw(IPlayer[] tiedPlayers)
        {
        }

        private void OnNewDeal()
        {
        }
        private void OnSelectCardsToDiscard(IPlayer player)
        {
        }
        private void OnShowAllHands()
        {
        }
    }
}