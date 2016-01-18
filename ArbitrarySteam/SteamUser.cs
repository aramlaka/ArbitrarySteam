using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArbitrarySteam
{
    class SteamUser
    {
        public bool BadProfile { get; set; }

        public string URL { get; set; }
        public string SteamID { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public string GamesString { get; set; }

        public HashSet<SteamGame> Games = new HashSet<SteamGame>();

        public void GetGamesSetFromString()
        {
            string temp = GamesString;
            for(int i = 0; i < temp.Length && i != -1; i = temp.IndexOf("<message>"))
            {
                string message = Utilities.ParseXML(temp, "message");
                string game = Utilities.ParseXML(message, "appid");
                string playtime = Utilities.ParseXML(message, "playtime_forever");

                int playtimeInt;
                try
                {
                    playtimeInt = Int32.Parse(playtime);
                }
                catch(Exception ex) // :(
                {
                    Console.WriteLine(ex.Message);
                    BadProfile = true;
                    break;
                }
                

                Games.Add(new SteamGame(game, Int32.Parse(playtime)));

                

                temp = temp.Substring(temp.IndexOf("</message>") + 1);
            }
        }
    }
}
