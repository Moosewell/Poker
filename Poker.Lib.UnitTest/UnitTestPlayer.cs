using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Poker.Lib.UnitTest
{
    public class PlayerTests
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
            cards = TestMethods.ToCards("♥10♥J♥Q♥K♥A");
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
            cards = TestMethods.ToCards("♥10♦J♥Q♥K♣A♣4");
            Player player = new Player("Gustav");
            CollectionAssert.AreEqual(player.Hand, new Card[] {null, null, null, null, null});
            Assert.Throws<IndexOutOfRangeException>(() => player.DrawCards(cards));
            
        }

        [Test]
        public void CardsCanBeDiscarded()
        {
            //assemble
            var cards = new Card[5];
            cards = TestMethods.ToCards("♥10♥J♥Q♥K♥A");
            Player player = new Player("Gustav");
            player.DrawCards(cards);
            var cardsToDiscard = TestMethods.ToCards("♥10♥K♥A");
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
            cards = TestMethods.ToCards("♣10♠J♥Q♦K♦10");
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
            cards = TestMethods.ToCards("♣10♠J♥Q♦K♦10");
            Player player = new Player("Gustav");
            player.DrawCards(cards);
            player.Discard = TestMethods.ToCards("♣K♠2♥3");
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
            Suite suite = (Suite)TestMethods.RandomSuite();
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
            Suite suite = (Suite)TestMethods.RandomSuite();
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
            int randomRank = TestMethods.RandomRank();
            var randomFOK = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomFOK);
            player.DrawCards(TestMethods.DealTopCards(1, smallTestDeck));
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
                    int tempNum = TestMethods.RandomRank();
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
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank1).ToArray();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank2).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
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
            for(var i = 0; i < 1000; i++)
            {
            //assemble
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
            int randomRank = TestMethods.RandomRank();
            var randomTOK = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            Random random = new Random();
            while (randomTOK.Length > 3)
            {
                int removeCard = random.Next(0, randomTOK.Length);
                randomTOK[removeCard] = null;
                randomTOK = randomTOK.Where(card => card != null).ToArray();
            }
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomTOK);
            player.DrawCards(TestMethods.DealTopCards(2, smallTestDeck));
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
                    int tempNum = TestMethods.RandomRank();
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
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank1).ToArray();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank2).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomPair1);
            player.DrawCards(randomPair2);
            player.DrawCards(TestMethods.DealTopCards(1, smallTestDeck));
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
            int randomRank = TestMethods.RandomRank();
            var randomPair = dealer.Deck.Where(card => (int)card.Rank == randomRank).ToArray();
            Random random = new Random();
            while (randomPair.Length > 2)
            {
                int removeCard = random.Next(0, randomPair.Length);
                randomPair[removeCard] = null;
                randomPair = randomPair.Where(card => card != null).ToArray();
            }
            var smallTestDeck = TestMethods.OneOfEachRankDeck();
            smallTestDeck = smallTestDeck.Where(card => (int)card.Rank != randomRank).ToArray();
            smallTestDeck = TestMethods.Shuffle(smallTestDeck);
            Player player = new Player("Gustav");
            player.DrawCards(randomPair);
            player.DrawCards(TestMethods.DealTopCards(3, smallTestDeck));
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
    }
}