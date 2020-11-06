namespace Poker.Lib
{
    public static class GameFactory
    {
        public static IPokerGame NewGame(string[] playerNames)
        {
            PokerGame Game = new PokerGame(playerNames);
            return Game;
        }

        public static IPokerGame LoadGame(string fileName)
        {
            return null;
        }
    }
}