namespace TeamAssigner.Services
{
    using System.Text;
    using System.Text.Json;
    using TeamAssigner.Models;
    using System.Linq;
   
    public sealed class TeamRandomizer
    {
        //To Test:
            //Hardcode Year and Week
            //Comment out call the GetNFLWeek() method
            //Don't forget to undo testing changes

        readonly EmailService? emailService;
        readonly IList<PlayerInfo> players;
        readonly string weekOverride;
        readonly string baseurl;
        readonly string scoresBaseURL;
        readonly string quoteurl;
        readonly string adminEmail;
        int year = DateTime.Now.Year;
        int week = 0;

        public TeamRandomizer(IList<PlayerInfo> players, EmailService? emailService, string adminEmail, string baseurl, string scoresBaseURL, string quoteurl, string weekOverride = "")
        {
            try
            {
                this.baseurl = baseurl;
                this.scoresBaseURL = scoresBaseURL;
                this.quoteurl = quoteurl;
                this.players = players?.ToList();
                this.emailService = emailService;
                this.adminEmail = adminEmail;

            }
            catch (Exception e)
            {
                weekOverride = "";
                players = [];
                Exit($"An error occurred starting the app.", true, e);
            }
        }

        public void Run()
        {
            // MAIN
            GetNFLWeek();

            if (players?.Count == 16 || players?.Count == 32)
            {
                Console.WriteLine($"year: {year} week: {week}\n");
                if (week > 0 && week < 19)
                {
                    StringBuilder thisweekSB = Randomize();
                    StringBuilder lastweekSB = GetPreviousWeekScores(week);
                    emailService?.SendEmail(String.Join(",", players.Select(p => p.Email)), $"Week {week}", thisweekSB.ToString() + lastweekSB.ToString());
                }
                else
                {
                    Exit($"Not in a regular season week.", false);
                }
            }
            else
            {
                Exit($"For equal distribution of byes there must be either 16 or 32 players. There are currently {players?.Count} players.", true);
            }

        }

        private void GetNFLWeek()
        {
            try
            {
                // DETERMINE NFL YEAR
                DateTime now = DateTime.Now;
                string seasonJson = RESTUtil.Get([], $"{baseurl}/seasons/{now.Year}/types/2");
                seasonJson = seasonJson.Replace("$ref", "reference");
                NFLSeason? seasonResults = JsonSerializer.Deserialize<NFLSeason>(seasonJson);

                DateTime start = Convert.ToDateTime(seasonResults?.startDate);
                DateTime end = Convert.ToDateTime(seasonResults?.endDate);

                if (now.Ticks > start.Ticks)
                {
                    if (now.Ticks < end.Ticks)
                    {
                        year = now.Year;
                    }
                    else
                    {
                        year = now.Year + 1;
                    }
                }
                else
                {
                    year = now.Year - 1;
                }

                // DETERMINE NFL WEEK
                if (String.IsNullOrWhiteSpace(weekOverride))
                {
                    string weeksJson = RESTUtil.Get([], $"{baseurl}/seasons/{year}/types/2/weeks");
                    weeksJson = weeksJson.Replace("$ref", "reference");
                    NFLObject? weeksResults = JsonSerializer.Deserialize<NFLObject>(weeksJson);
                    if (weeksResults != null)
                    {
                        foreach (var weekInfo in weeksResults.items)
                        {
                            string weekJson = RESTUtil.Get([], $"{weekInfo.reference}");
                            weekJson = weekJson.Replace("$ref", "reference");
                            NFLWeek? weekResult = JsonSerializer.Deserialize<NFLWeek>(weekJson);
                            if (weekResult != null)
                            {
                                Console.WriteLine($"Checking if we are in {weekResult.text} -- start: {weekResult.startDate} end: {weekResult.endDate}...");
                                if (Convert.ToDateTime(weekResult.startDate) < now)
                                {
                                    if (now < Convert.ToDateTime(weekResult.endDate))
                                    {
                                        Console.WriteLine($"Setting week to {weekResult.number}.");
                                        week = weekResult.number;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
                else
                {
                    bool success = Int32.TryParse(weekOverride, out week);
                    {
                        Console.WriteLine($"Could not convert '{weekOverride}' to a number.");
                    }
                }
            }
            catch (Exception ex)
            {
                Exit($"An error occurred getting the NFL Season and Week info.", true, ex);
            }
        }

        private StringBuilder GetPreviousWeekScores(int week)
        {
            string scoresJson = RESTUtil.Get([], $"{scoresBaseURL}/scoreboard?dates={year}&seasontype=2&week={week-1}");
            scoresJson = scoresJson.Replace("$ref", "reference");
            NFLWeeklyScores? scoreInfo = JsonSerializer.Deserialize<NFLWeeklyScores>(scoresJson);

            StringBuilder sb = new();
            foreach (var game in scoreInfo?.events)
            {
                foreach (var competition in game.competitions) {
                    foreach (var team in competition.competitors)
                    {
                        if (team.score == "33")
                        {
                            sb.AppendLine("");
                            sb.AppendLine($"Congratulation to the player who had the {team.team.displayName} last week.");
                            sb.AppendLine(competition.headlines[0].shortLinkText);
                            sb.AppendLine("");
                            Console.WriteLine($"{team.team.displayName} had {team.score} in week {week-1}.");
                        }
                    }
                }
            }
            if (sb.Length < 1)
            {
                sb.AppendLine("");
                sb.AppendLine("No team had 33 points last week.");
                sb.AppendLine("");
                try
                {
                    string quoteJson = RESTUtil.Get([], quoteurl);
                    Quote? quoteObj = JsonSerializer.Deserialize<Quote>(quoteJson);
                    sb.AppendLine($"{quoteObj?.quote} - {quoteObj?.author}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to get quote");
                }
            }

            return sb;
        }


        private List<string> GetByeTeams(int week)
        {
            string weekJson = RESTUtil.Get([], $"{baseurl}/seasons/{year}/types/2/weeks/{week}");
            weekJson = weekJson.Replace("$ref", "reference");
            NFLWeekDetails? weekInfo = JsonSerializer.Deserialize<NFLWeekDetails>(weekJson);

            List<string> byeTeams = [];
            if (weekInfo?.teamsOnBye != null)
            {
                foreach (var byeTeam in weekInfo.teamsOnBye)
                {
                    string byeTeamJson = RESTUtil.Get([], byeTeam.reference);
                    NFLTeam? byeTeamResult = JsonSerializer.Deserialize<NFLTeam>(byeTeamJson);
                    if (byeTeamResult?.nickname != null)
                    {
                        byeTeams.Add(byeTeamResult.nickname);
                    }
                }
            }
            return byeTeams;
        }

        private int GetByeTeamCountToDate()
        {
            int count = 0;
            for (int i = week - 1; i > 0; i--)
            {
                count += GetByeTeams(i).Count;
            }

            return count;
        }

        private List<string> GetAllTeams()
        {
            var allTeams = new List<String>();
            string teamsJson = RESTUtil.Get([], $"{baseurl}/teams?limit=32");
            teamsJson = teamsJson.Replace("$ref", "reference");
            NFLObject? teamsResult = JsonSerializer.Deserialize<NFLObject>(teamsJson);
            foreach (var team in teamsResult?.items)
            {
                string teamJson = RESTUtil.Get([], team.reference);
                NFLTeam? teamResult = JsonSerializer.Deserialize<NFLTeam>(teamJson);
                if (teamResult?.nickname != null)
                {
                    allTeams.Add(teamResult.nickname);
                }
            }

            return allTeams;
        }

        private StringBuilder Randomize()
        {
            StringBuilder sb = new();
            try
            {
                var rnd = new Random();
                var allTeams = GetAllTeams();
                var byeTeams = GetByeTeams(week);
                byeTeams.ForEach(p => Console.WriteLine($"Bye Team: {p}"));

                //There are teams on bye so need to evenly distribute those teams
                if (byeTeams.Count != 0)
                {
                    //Setup Lists and Queues
                    Queue<string> randomNonByeTeams = new();
                    var orderedPlayers = players?.OrderBy(p => p.ID);

                    var nonByeTeams = allTeams.Where(x => !byeTeams.Contains(x)).ToList();
                    nonByeTeams.OrderBy(item => rnd.Next()).Distinct().ToList().ForEach(i => randomNonByeTeams.Enqueue(i));

                    int byeMarker = GetByeTeamCountToDate();
                    Console.WriteLine($"Bye Team Count To Date: {byeMarker}\n");

                    //Assign the new bye teams to the next player ids in consecutive order
                    if (byeMarker >= 16 && players?.Count <= 16)
                    {
                        byeMarker -= 16;
                        Console.WriteLine($"Bye Team Marker Set To: {byeMarker}\n");
                    }
                    foreach (var byeTeamName in byeTeams)
                    {
                        byeMarker++;

                        var player = orderedPlayers?.Where(p => p.ID.Equals(byeMarker)).First();
                        sb.AppendLine($"{player?.Name}: {byeTeamName.ToUpper()} & {randomNonByeTeams.Dequeue()}");

                        player.Filled = true;
                    }

                    //Fill out the rest of the teams
                    foreach (var player in orderedPlayers)
                    {
                        if (!player.Filled)
                        {
                            sb.AppendLine($"{player.Name}: {randomNonByeTeams.Dequeue()} & {randomNonByeTeams.Dequeue()}");
                        }
                    }
                }
                //No bye teams so evenly distribute all teams
                else
                {
                    Queue<string> randomTeams = new();
                    allTeams.OrderBy(item => rnd.Next()).Distinct().ToList().ForEach(i => randomTeams.Enqueue(i));
                    var randomPlayers = players?.OrderBy(item => rnd.Next());

                    foreach (var player in randomPlayers)
                    {
                        sb.AppendLine($"{player.Name}: {randomTeams.Dequeue()} & {randomTeams.Dequeue()}");
                    }
                }

                Console.WriteLine(sb.ToString());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Exit($"An error occurred running the app.", true, ex);
            }
            return sb;
        }

        private void Exit(string msg, bool sendEmail, Exception? ex = null)
        {
            Console.WriteLine(msg);
            if (ex != null)
            {
                Console.WriteLine(ex.ToString());
            }
            if (sendEmail)
            {
                emailService?.SendEmail(adminEmail, "Error Running NFL Team Assigner", $"{msg} {ex?.ToString()}");
            }
        }
    }
}
