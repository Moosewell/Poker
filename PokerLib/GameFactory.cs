using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Poker.Lib
{
    public static class GameFactory
    {
        public static IPokerGame NewGame(string[] playerNames)
        {
            IPokerGame Game = new PokerGame(playerNames);
            return Game;
        }

        public static IPokerGame LoadGame(string fileName)
        {
            IPlayer[] loadFile;
            Stream stream = File.Open(fileName, FileMode.Open);
            IFormatter formatter = new BinaryFormatter();
            loadFile = (IPlayer[])formatter.Deserialize(stream);
            stream.Close();
            IPokerGame Game = new PokerGame(loadFile);
            return Game;
        }
    }
}