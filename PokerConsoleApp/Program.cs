using System;
using System.IO;

namespace Poker.ConsoleApp
{
    class Program
    {
        private static Lib.IPokerGame game;

        private static void OnNewDeal()
        {
            UserInterface.AnnounceNewDeal();
        }

        private static void OnSelectCardsToDiscard(IPlayer player)
        {
            player.Discard = UserInterface.SelectCardsToDiscard(player);
        }

        private static void OnRecievedReplacementCards(IPlayer player)
        {
            UserInterface.InspectReplacementCards(player);
        }

        private static void OnShowAllHands()
        {
            foreach(var player in game.Players)
            {
                UserInterface.ShowHand(player);
            }
        }

        private static void OnWinner(IPlayer player)
        {
            UserInterface.PresentWinner(player);
            PresentStats();
        }

        private static void OnDraw(IPlayer[] tiedPlayers)
        {
            UserInterface.DeclareDraw(tiedPlayers);
            PresentStats();
        }

        private static void PresentStats()
        {
            if (!UserInterface.PresentStats(game.Players))
            {
                if (Char.ToLower(
                        UserInterface.WaitForKey("Spara spelet? [J/N]").KeyChar) 
                    == 'j')
                {
                    game.SaveGameAndExit("savedgame.txt");
                }
                else 
                {
                    game.Exit();
                }
            }
        }

        static void Main(string[] args)
        {
            game = (File.Exists("savedgame.txt") 
                && Char.ToLower(UserInterface.WaitForKey(
                        "Ladda sparat spel? [J/N]").KeyChar) 
                    == 'j')
                ? Lib.GameFactory.LoadGame("savedgame.txt")
                : Lib.GameFactory.NewGame(UserInterface.RegisterPlayers());           
            
            game.NewDeal += OnNewDeal;
            game.SelectCardsToDiscard += OnSelectCardsToDiscard;
            game.RecievedReplacementCards += OnRecievedReplacementCards;
            game.ShowAllHands += OnShowAllHands;
            game.Winner += OnWinner;
            game.Draw += OnDraw;
            
            game.RunGame();   
        }

    }
}
