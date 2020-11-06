using System;

namespace Poker.Lib
{
    class PokerGame : IPokerGame
    {
        public IPlayer[] Players { get; set;}

        public PokerGame(string[] playernames)
        {
            Players = new IPlayer[playernames.Length];
            for(var i = 0; i < playernames.Length; i++)
            {
                Players[i] = new Player(playernames[i]);
            }
        }

        public void RunGame()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Dealer dealer = new Dealer();
            dealer.Shuffle();
            NewDeal();
            dealer.DealHands(Players);
            foreach(Player player in Players)
            {
                player.SortHand();
                SelectCardsToDiscard(player);
                player.DiscardCards();
                player.DrawCards(dealer.DealTopCards(player.Discard.Length));
                player.SortHand();
                RecievedReplacementCards(player);
                player.IdentifyHand();
            }
            ShowAllHands();
            //Compare hands
            //Display winner/tie and stats
            dealer.Restore(Players);
            //Continue/Quit game/Save and quit.
        }
        public void Exit()
        {
            
        }

        public void SaveGameAndExit(string fileName)
        {
            
        }

        public void Handlerestult(IPlayer[] results)
        {
            
        }

        

        public event OnNewDeal NewDeal;

        public event OnSelectCardsToDiscard SelectCardsToDiscard;

        public event OnRecievedReplacementCards RecievedReplacementCards;

        public event OnShowAllHands ShowAllHands;

        public event OnWinner Winner;

        public event OnDraw Draw;
    }
}