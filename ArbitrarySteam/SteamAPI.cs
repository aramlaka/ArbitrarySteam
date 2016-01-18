using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArbitrarySteam
{
    class SteamAPI
    {
        private string apiKey; //Get this at http://steamcommunity.com/dev/apikey

        public SteamUser User { get; set; }
        public bool NoSteamKey { get; set; }

        public SteamAPI(string profileURL, bool isCustomURL)
        {
            if(String.IsNullOrEmpty(apiKey))
            {
                NoSteamKey = true;
            }

            User = new SteamUser();
            User.URL = "http://www." + profileURL;

            string profileInfo;

            if(isCustomURL)
            {
                User.Name = RemoveExcessURL(profileURL, isCustomURL);
                User.SteamID = GetSteamIDFromVanityName(User.Name);

                if(String.IsNullOrEmpty(User.SteamID)) //given an invalid vanity name
                {
                    User.BadProfile = true;
                }
                else //no point in trying these requests if we know they will fail
                {
                    profileInfo = RequestPlayerSummary(User.SteamID);
                    User.AvatarUrl = Utilities.ParseXML(profileInfo, "avatarmedium");  
                }                              
            }
            else
            {
                User.SteamID = RemoveExcessURL(profileURL, isCustomURL);
                profileInfo = RequestPlayerSummary(User.SteamID);
                User.Name = Utilities.ParseXML(profileInfo, "personaname");

                if (String.IsNullOrEmpty(User.Name)) //given an invalid steam id
                {
                    User.BadProfile = true;
                }
                else
                {
                    User.AvatarUrl = Utilities.ParseXML(profileInfo, "avatarmedium");
                }                
            }

            if(!User.BadProfile)
            {
                User.GamesString = RequestSteamGamesListString(User.SteamID);
                User.GetGamesSetFromString();
            }

            //Console.WriteLine(String.Format("\"{0}\" \"{1}\" \n\"{2}\" \n\"{3}\" \n\"{4}\"", User.Name, User.SteamID, User.URL, User.AvatarUrl, User.GamesString)); 
            Console.WriteLine("Finished with SteamAPI constructor");
        }

        public static string GetAppNameFromId(string appID)
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    string name = "", apiResponse = Client.DownloadString(String.Format("http://store.steampowered.com/api/appdetails/?appids={0}&filters=basic", appID));

                    //TODO: IMPLEMENT THIS ON OWN
                    //The next few lines are from this:
                    //https://github.com/jshackles/idle_master/blob/master/Source/IdleMaster/frmMain.cs#L52-56

                    if (System.Text.RegularExpressions.Regex.IsMatch(apiResponse, "\"game\",\"name\":\"(.+?)\""))
                    {
                        name = System.Text.RegularExpressions.Regex.Match(apiResponse, "\"game\",\"name\":\"(.+?)\"").Groups[1].Value;
                    }

                    name = System.Text.RegularExpressions.Regex.Unescape(name);

                    //we were able to get the name, yet there isn't one.  this means the game is no longer supported. EX. an alpha or beta of a game
                    if (String.IsNullOrEmpty(name)) { return "App no longer supported"; }


                    return name;
                }
            }
            catch (Exception except) { Console.WriteLine(except.Message); }


            return String.Empty;
        }

        private string RequestSteamGamesListString(string steamID)
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    return Client.DownloadString(String.Format("http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&steamid={1}&format=xml", apiKey, steamID));
                }
            }
            catch (Exception except) { Console.WriteLine(except.Message); }

            return String.Empty;
        }

        private string GetSteamIDFromVanityName(string vanityName)
        {
            string xml = RequestSteamID(vanityName);
            if(Utilities.ParseXML(xml, "success") == "1")
            {
                return Utilities.ParseXML(xml, "steamid");
            }

            return String.Empty;
        }

        private string RequestSteamID(string vanityName)
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    return Client.DownloadString(String.Format("http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}&format=xml", apiKey, vanityName));
                }
            }
            catch (Exception except) { Console.WriteLine(except.Message); }

            return String.Empty;
        }

        private string RequestPlayerSummary(string steamID)
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    return Client.DownloadString(String.Format("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}&format=xml", apiKey, steamID));
                }
            }
            catch (Exception except) { Console.WriteLine(except.Message); }

            return String.Empty;
        }


        private string RemoveExcessURL(string profileURL, bool isCustomURL)
        {
            int startIndex;

            if(isCustomURL) //profileURL is in format "steamcommunity.com/id/THEIRVANITYNAME"
            {
                startIndex = profileURL.IndexOf(".com/id/") + ".com/id/".Length;
            }
            else //profileURL is in format "steamcommunity.com/profiles/THEIRSTEAM64ID"
            {
                startIndex = profileURL.IndexOf(".com/profiles/") + ".com/profiles/".Length;
            }

            return profileURL.Substring(startIndex);
        }
    }
}
