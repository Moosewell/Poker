using NUnit.Framework;

namespace Poker.Lib.UnitTest
{
    public class DealerTests
    {
        [SetUp]
        public void Setup()
        {
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
    }
}