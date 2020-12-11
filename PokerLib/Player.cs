using System.Linq;
using System;

namespace Poker
{
    [Serializable]
    public class Player : IPlayer
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
            Discard = Discard.Where(card => card != null).ToArray();
        }

        public void DrawCards(ICard[] newCards)
        {
            Hand = Hand.Where(card => card != null).ToArray();
            Hand = Hand.Concat(newCards).ToArray();
            if(Hand.Length > 5)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void SortHand()
        {
            Hand = Hand.OrderBy(card => card.Suite).ToArray();
            Hand = Hand.OrderBy(card => card.Rank).ToArray();
        }

        public void ClearPiles()
        {
            Discard = new ICard[5];
            Hand = new ICard[5];
        }
        
        public void IdentifyHand()
        {
            bool lowestCard10 = (int)Hand[0].Rank == 10 ? true : false;
            bool isFourOfAKind = false;
            bool isFullHouse = false;
            bool isFlush = true;
            bool isStraight = true;
            bool isThreeOfAKind = false;
            int pairs = 0;

            int matchingRank1 = 0;
            int matchingRank2 = 0;
            bool firstMatch1 = true;
            bool firstMatch2 = true;
            Rank savedRank = 0;
            bool canSaveRank = true;

            for(var card = 0; card < 4; card++)
            {
                savedRank = canSaveRank ? Hand[card].Rank : savedRank;

                if(Hand[card].Rank == Hand[card + 1].Rank && Hand[card].Rank == savedRank)
                {
                    matchingRank1 += firstMatch1 ? 2 : 1;
                    canSaveRank = false;
                    firstMatch1 = false;
                }
                if(Hand[card].Rank == Hand[card + 1].Rank && Hand[card].Rank != savedRank)
                {
                    matchingRank2 += firstMatch2 ? 2 : 1;                   
                    firstMatch2 = false;
                }
            }

            switch (matchingRank1)
            {
                case 2:
                pairs++;
                break;
                case 3:
                isThreeOfAKind = true;
                break;
                case 4:
                isFourOfAKind = true;
                break;
                default:
                break;
            }

            switch (matchingRank2)
            {
                case 2:
                pairs++;
                break;
                case 3:
                isThreeOfAKind = true;
                break;
                default:
                break;
            }

            if(isThreeOfAKind && pairs == 1)
            {
                isFullHouse = true;
            }

            for(var card = 0; card < 4; card++)
            {
                if((int)Hand[card].Rank != (int)Hand[card + 1].Rank - 1)
                {
                    isStraight = false;
                }
            }

            for(var card = 0; card < 4; card++)
            {
                if(Hand[card].Suite != Hand[card + 1].Suite)
                {
                    isFlush = false;
                }
            }

            if((int)Hand[0].Rank == 2 && (int)Hand[1].Rank == 3 && (int)Hand[2].Rank == 4 && 
            (int)Hand[3].Rank == 5 && (int)Hand[4].Rank == 14)
            {
                isStraight = true;

                ICard[] temp = new ICard[1];
                temp[0] = Hand[4];
                Hand[4] = null;
                Hand = Hand.Where(card => card != null).ToArray();
                temp = temp.Concat(Hand).ToArray();
                Hand = temp;
                
            }

            switch(lowestCard10, isFourOfAKind, isFullHouse, isFlush, isStraight, isThreeOfAKind, pairs)
            {
                case var state 
                when state.lowestCard10 == true &&  state.isFlush == true && state.isStraight == true:
                this.HandType = HandType.RoyalStraightFlush;
                break;
                case var state 
                when state.isFlush == true && state.isStraight == true:
                this.HandType = HandType.StraightFlush;
                break;
                case var state 
                when state.isFourOfAKind == true:
                this.HandType = HandType.FourOfAKind;
                break;
                case var state 
                when state.isFullHouse == true:
                this.HandType = HandType.FullHouse;
                break;
                case var state 
                when state.isFlush == true:
                this.HandType = HandType.Flush;
                break;
                case var state 
                when state.isStraight == true:
                this.HandType = HandType.Straight;
                break;
                case var state 
                when state.isThreeOfAKind == true:
                this.HandType = HandType.ThreeOfAKind;
                break;
                case var state 
                when state.pairs == 2:
                this.HandType = HandType.TwoPairs;
                break;
                case var state 
                when state.pairs == 1:
                this.HandType = HandType.Pair;
                break;
                default:
                this.HandType = HandType.HighCard;
                break;
            }
        }
    }
}