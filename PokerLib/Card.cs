using System;

namespace Poker
{
    [Serializable]
    class Card : ICard
    {
        public Card(Suite suite, Rank rank)
        {
            Suite = suite;
            Rank = rank;
        }
        public Suite Suite { get; set; }

        public Rank Rank { get; set; }
    }
}