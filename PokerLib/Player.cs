using System.Linq;

namespace Poker
{
    class Player : IPlayer
    {
        public string Name { get; set; }

        public ICard[] Hand { get; set; }

        public HandType HandType { get; set; }

        public int Wins {get; set;}

        public ICard[] Discard { get; set; }

        public Player(string name)
        {
            Name = name;
            Wins = 0;
            Discard = new ICard[5];
            Hand = new ICard[5];
        }

        public void DiscardCards()
        {
            Hand = Hand.Except(Discard).ToArray();
        }

        public void DrawCards(ICard[] newCards)
        {
            Hand = Hand.Where(card => card != null).ToArray();
            Hand = Hand.Concat(newCards).ToArray();
        }

        public void SortHand()
        {
            Hand = Hand.OrderBy(card => card.Suite).ToArray();
            Hand = Hand.OrderBy(card => card.Rank).ToArray();
        }
        
        public void IdentifyHand()
        {
            bool lowestCard10 = (int)Hand[0].Rank == 10 ? true : false;
        
            bool isFourOfAKind;
            bool isFullHouse;
            bool isFlush;
            bool isStraight;
            bool isThreeOfAKind;
            bool isTwoPair;
            bool isPair = false;
            int sameRank = 0;
            Rank pairValue = 0;
            
            for(var card = 0; card < 5; card++)
            {
                if(Hand[card].Rank == Hand[card + 1].Rank)
                {
                    if(!isPair)
                    {
                        pairValue = Hand[card].Rank;
                        isPair = true;
                        sameRank = 2;
                    }
                    else if (Hand[card].Rank == pairValue)
                    {
                        sameRank++;
                        isPair = false;
                        isThreeOfAKind = true;
                    }
                    else if (Hand[card].Rank != pairValue)
                    {
                        isTwoPair = true;
                        {

                        }
                    }
                }
            }
        }
    }
}