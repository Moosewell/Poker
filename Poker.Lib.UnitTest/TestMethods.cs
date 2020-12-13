using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace Poker.Lib.UnitTest
{
    static class TestMethods
    {
    static public int RandomSuite()
    {
        Random random = new Random();
        return random.Next(0, 4);
    }

    public static int RandomRank()
    {
        Random random = new Random();
        return random.Next(2, 15);
    }

    public static ICard[] OneOfEachRankDeck()
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

    public static ICard[] Shuffle(ICard[] inputDeck)
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

        public static ICard[] DealTopCards(int amountToDraw, ICard[] inputDeck)
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

    public static Card[] ToCards(string text) {
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
        public static void OnWinner(IPlayer player)
        {
        }

        public static void OnDraw(IPlayer[] tiedPlayers)
        {
        }

        public static void OnNewDeal()
        {
        }
        public static void OnSelectCardsToDiscard(IPlayer player)
        {
        }
        public static void OnShowAllHands()
        {
        }
    }
}