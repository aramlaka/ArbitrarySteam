using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArbitrarySteam
{
    class SteamGame
    {
        public string Name { get; set; }
        public string AppID { get; set; }
        public double HoursPlayed { get; set; } 

        public SteamGame(string appID, double hoursPlayed)
        {
            AppID = appID;
            HoursPlayed = hoursPlayed;
        }
       
    }
}
