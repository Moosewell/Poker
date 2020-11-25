using NUnit.Framework;
using Poker;
using Poker.Lib;

namespace Poker.Lib.UnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RoyalStraightFlush()
        {
            Card[] cards = new Card[] {"♥10♥J♥Q♥K♥A"};
            Player player = new Player();
            CollectionAssertion.IsEmpty(player);
            for (int i = 0; i < 5; ++i) {
        player.AddCard(cards[i]);
        CollectionAssert.AreEqual(cards.Take(i + 1), player);
        }
    }

    static Card[] ToCards(string text) {
    List<Card> cards = new List<Card>();
    int i = 0; 
    while(i < text.Length) {
        Suite suite = (text[i]) switch {
            '♣' => Clubs, '♦' => Diamonds, '♥' => Hearts, '♠'=> Spades, 
            _ => throw new NotImplementedException(),
        };
        var rankString = text.Substring(i + 1);
        var rankFunc = new Dictionary<string, Func<string, Rank>>() {
            {@"^J",  _ => Jack}, {@"^Q", _ => Queen}, {@"^K", _ => King}, 
            {@"^A", _ => Ace}, { @"^\d+", str => (Rank)int.Parse(str) }
        };
        var func = rankFunc.Where(func => Regex.IsMatch(rankString, func.Key)).First();
        cards.Add((suite, func.Value(Regex.Match(rankString, func.Key).Value)));
        i += Regex.IsMatch(rankString, @"^\d\d") ? 3 : 2;
        }
    return cards.ToArray();
    }
    }
}