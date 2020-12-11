using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Poker.Lib.UnitTest")]
namespace Poker.Lib
{
    class PokerGame : IPokerGame
    {
        public IPlayer[] Players { get; set;}

        public bool gameAlive { get; set;}

        public PokerGame(string[] playernames)
        {
            Players = new IPlayer[playernames.Length];
            for(var i = 0; i < playernames.Length; i++)
            {
                Players[i] = new Player(playernames[i]);
            }
        }

        public PokerGame(IPlayer[] savedPlayers)
        {
            Players = savedPlayers;
        }

        public void RunGame()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Dealer dealer = new Dealer();
            gameAlive = true;
            while (gameAlive)
            {
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
                if(player.Discard.Length > 0)
                {
                RecievedReplacementCards(player);
                }
                player.IdentifyHand();
            }
            ShowAllHands();
            HandleResult(Players, dealer);
            }
        }
        public void Exit()
        {
            gameAlive = false;
        }

        public void SaveGameAndExit(string fileName)
        {
            object savefile = Players;
            Stream stream = File.Create(fileName);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, Players);
            stream.Close();
            gameAlive = false;
        }

        public void HandleResult(IPlayer[] players, Dealer dealer)
        {
            List<IPlayer> sameHandType = new List<IPlayer>();
            List<IPlayer> bestHand = new List<IPlayer>();
            players = players.OrderByDescending(player => player.HandType).ToArray();
            HandType bestHandType = players[0].HandType;
            foreach(IPlayer player in players)
            {
                if(player.HandType == bestHandType)
                {
                    sameHandType.Add(player);
                }
            }

            sameHandType = sameHandType.OrderByDescending(player => player.Hand[4].Rank).ToList();

            switch (sameHandType[0].HandType)
            {
                case var handType when handType == HandType.RoyalStraightFlush:
                foreach (IPlayer player in sameHandType)
                bestHand.Add(player);
                break;
                
                case var handType when handType == HandType.StraightFlush || handType == HandType.Straight:
                bestHand.Add(sameHandType[0]);
                for(var player = 0; player < sameHandType.Count - 2; player++)
                {   
                    if(sameHandType[player].Hand[4].Rank == sameHandType[player + 1].Hand[4].Rank)
                    {
                        bestHand.Add(sameHandType[player + 1]);
                    }
                }
                break;

                case var handType when handType == HandType.Flush:
                IPlayer bestPlayer = sameHandType[0];
                for(var player = 1; player < sameHandType.Count - 2; player++)
                {
                    int sameRank = 0;
                    for(var card = 4; card >= 0; card--)
                    {
                        if(bestPlayer.Hand[card].Rank <= sameHandType[player].Hand[card].Rank)
                        {
                            bestPlayer = sameHandType[player];
                        }
                        else
                        {
                            sameRank++;
                        }
                    }
                    if(sameRank == 5)
                    {
                        bestHand.Add(sameHandType[player]);
                    }
                }
                bestHand.Add(bestPlayer);
                break;

                case var handType when handType == HandType.FullHouse || handType == HandType.ThreeOfAKind || handType == HandType.FourOfAKind:
                bestPlayer = sameHandType[0];
                foreach (IPlayer player in sameHandType)
                {
                    if(bestPlayer.Hand[2].Rank < player.Hand[2].Rank)
                    {
                        bestPlayer = player;
                    }
                }
                bestHand.Add(bestPlayer);
                break;

                case var handType when handType == HandType.TwoPairs:
                Rank[][] playerHandInfo = new Rank[sameHandType.Count][];
                for (var player = 0; player < sameHandType.Count; player++)
                {
                    Rank[] handInfo = new Rank[3];
                    playerHandInfo[player] = handInfo;
                    Rank pair1 = sameHandType[player].Hand[1].Rank;
                    Rank pair2 = sameHandType[player].Hand[3].Rank;
                    handInfo[0] = pair1 > pair2 ? pair1 : pair2;
                    handInfo[1] = pair1 < pair2 ? pair1 : pair2;
                    foreach(ICard card in sameHandType[player].Hand)
                    {
                        if(card.Rank != pair1 && card.Rank != pair2)
                        {
                            handInfo[2] = card.Rank;
                        }
                    }
                }
                Rank[] bestHandInfo = playerHandInfo[0];
                bestPlayer = sameHandType[0];
                List<IPlayer> sameHandInfo = new List<IPlayer>();
                for (var player = 0; player < sameHandType.Count; player++)
                {
                    if(bestHandInfo[0] < playerHandInfo[player][0])
                    {
                        bestHandInfo = playerHandInfo[player];
                        bestPlayer = sameHandType[player];
                        sameHandInfo.Clear();
                    }
                    else if (bestHandInfo[0] == playerHandInfo[player][0])
                    {
                        if(bestHandInfo[1] < playerHandInfo[player][1])
                        {
                            bestHandInfo = playerHandInfo[player];
                            bestPlayer = sameHandType[player];
                            sameHandInfo.Clear();
                        }
                        else if (bestHandInfo[1] == playerHandInfo[player][1])
                        {
                            if(bestHandInfo[2] < playerHandInfo[player][2])
                            {
                                bestHandInfo = playerHandInfo[player];
                                bestPlayer = sameHandType[player];
                                sameHandInfo.Clear();
                            }
                            else if (bestHandInfo[2] == playerHandInfo[player][2])
                            {
                                if(bestPlayer != sameHandType[player])
                                    {
                                    sameHandInfo.Add(sameHandType[player]);
                                    }
                            }
                        }
                    }
                }
                bestHand.Add(bestPlayer);
                foreach(IPlayer player in sameHandInfo)
                {
                    bestHand.Add(player);
                }
                break;

                case var handType when handType == HandType.Pair:
                playerHandInfo = new Rank[sameHandType.Count][];
                for (var player = 0; player < sameHandType.Count; player++)
                {
                    Rank[] handInfo = new Rank[4];
                    playerHandInfo[player] = handInfo;
                    int buffer = 0;
                    Rank savedCard = 0;
                    for (var card = 4; card >= 0; card--)
                    {
                        if(card == 0)
                        {
                            handInfo[3] = sameHandType[player].Hand[card].Rank;
                        }
                        else if(sameHandType[player].Hand[card].Rank == sameHandType[player].Hand[card - 1].Rank)
                        {
                            handInfo[0] = sameHandType[player].Hand[card].Rank;
                            savedCard = sameHandType[player].Hand[card].Rank;
                        }
                        else if(sameHandType[player].Hand[card].Rank != savedCard)
                        {
                            buffer++;
                            handInfo[0 + buffer] = sameHandType[player].Hand[card].Rank;
                        }
                    }
                }
                bestHandInfo = playerHandInfo[0];
                bestPlayer = sameHandType[0];
                sameHandInfo = new List<IPlayer>();
                for (var player = 0; player < sameHandType.Count; player++)
                {
                    if(bestHandInfo[0] < playerHandInfo[player][0])
                    {
                        bestHandInfo = playerHandInfo[player];
                        bestPlayer = sameHandType[player];
                        sameHandInfo.Clear();
                    }
                    else if (bestHandInfo[0] == playerHandInfo[player][0])
                    {
                        if(bestHandInfo[1] < playerHandInfo[player][1])
                        {
                            bestHandInfo = playerHandInfo[player];
                            bestPlayer = sameHandType[player];
                            sameHandInfo.Clear();
                        }
                        else if (bestHandInfo[1] == playerHandInfo[player][1])
                        {
                            if(bestHandInfo[2] < playerHandInfo[player][2])
                            {
                                bestHandInfo = playerHandInfo[player];
                                bestPlayer = sameHandType[player];
                                sameHandInfo.Clear();
                            }
                            else if (bestHandInfo[2] == playerHandInfo[player][2])
                            {
                                if(bestHandInfo[3] < playerHandInfo[player][3])
                                {
                                    bestHandInfo = playerHandInfo[player];
                                    bestPlayer = sameHandType[player];
                                    sameHandInfo.Clear();
                                }
                                else if (bestHandInfo[3] == playerHandInfo[player][3])
                                {
                                    if(bestPlayer != sameHandType[player])
                                    {
                                    sameHandInfo.Add(sameHandType[player]);
                                    }
                                }
                            }
                        }
                    }
                }
                bestHand.Add(bestPlayer);
                foreach(IPlayer player in sameHandInfo)
                {
                    bestHand.Add(player);
                }
                break;

                case var handType when handType == HandType.HighCard:
                bestPlayer = sameHandType[0];
                foreach (IPlayer player in sameHandType)
                {
                    for(var card = 4; card >= 0; card--)
                    {
                        if(bestPlayer.Hand[card].Rank < player.Hand[card].Rank)
                        {
                            bestPlayer = player;
                            break;
                        }
                        if(bestPlayer.Hand[card].Rank > player.Hand[card].Rank)
                        {
                            break;
                        }
                    }   
                }
                var bestPlayerHand = bestPlayer.Hand.Select(c => c.Rank);
                foreach(IPlayer player in sameHandType)
                {
                    int matchingCards = 0;
                    for(var card = 0; card < 5; card++)
                    {
                        if(bestPlayer.Hand[card].Rank == player.Hand[card].Rank)
                        {
                            matchingCards++;
                        }
                    }   
                    if(matchingCards == 5)
                    {
                        bestHand.Add(player);
                    }
                }
                break;
            }
            
            if(bestHand.Count > 1)
            {
                dealer.Restore(Players);
                Draw(bestHand.ToArray()); 
            }
            else
            {
                foreach(Player player in players)
                {
                    if(player.Name == bestHand[0].Name && player.Hand == bestHand[0].Hand)
                    {
                        player.Wins++;
                        dealer.Restore(Players);
                        Winner(player);
                    }
                }
            }
        }

        

        public event OnNewDeal NewDeal;

        public event OnSelectCardsToDiscard SelectCardsToDiscard;

        public event OnRecievedReplacementCards RecievedReplacementCards;

        public event OnShowAllHands ShowAllHands;

        public event OnWinner Winner;

        public event OnDraw Draw;
    }
}