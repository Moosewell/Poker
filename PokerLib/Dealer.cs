using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker
{
    class Dealer
    {
        internal ICard[] Deck;
        public Dealer()
        {
            Deck = new ICard[52];
            int deckPosition = 0;
                for(var suite = 0; suite < 4; suite++)
                {
                    for(var rank = 2; rank < 15; rank++)
                    {
                        Deck[deckPosition] = new Card((Suite)suite, (Rank)rank);
                        deckPosition++;
                    }
                }
        }
        public void Shuffle()
        {
            ICard[] shuffledDeck = new ICard[52];
            Random RandomNumber = new Random();
            List<int> occupiedPositions = new List<int>();
            foreach (ICard Card in Deck)
            {
                bool SearchAvailablePos = true;
                do
                {
                    int randomPosition = RandomNumber.Next(0, 52);
                    if(!occupiedPositions.Contains(randomPosition))
                    {
                        shuffledDeck[randomPosition] = Card;
                        occupiedPositions.Add(randomPosition);
                        SearchAvailablePos = false;
                    }
                } while(SearchAvailablePos);
            }
            Deck = shuffledDeck;
        }

        public void Restore(IPlayer[] players)
        {
            Deck = Deck.Where(card => card != null).ToArray();
            foreach (Player player in players)
            {
                Deck = Deck.Concat(player.Hand.Where(card => card != null)).ToArray();
                Deck = Deck.Concat(player.Discard.Where(card => card != null)).ToArray();
                player.Hand = new ICard[5];
                player.Discard = new ICard[5];
            }
        }

        public void DealHands(IPlayer[] players)
        {
            for(var deal = 0; deal < 5; deal++)
            {
                foreach(Player player in players)
                {
                    player.DrawCards(DealTopCards(1));
                }
            }
        }

        public ICard[] DealTopCards(int amountToDraw)
        {
            List<ICard> Cards = new List<ICard>();
            for(var Drawn = 0; Drawn < amountToDraw; Drawn++)
            {
                Cards.Add(Deck[Deck.Length - 1]);
                Deck[Deck.Length - 1] = null;
                Deck = Deck.Where(Card => Card != null).ToArray();
            }
            return Cards.ToArray();
        }
    }
}
