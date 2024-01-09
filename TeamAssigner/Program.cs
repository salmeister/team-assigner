
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using TeamAssigner.Models;
using TeamAssigner.Services;
using System.Text.Json;

// INITIALIZE
EmailSettings emailSettings;
List<PlayerInfo> players;
int year = 2023;
int week = 6;
string weekOverride;
string baseurl = "http://sports.core.api.espn.com/v2/sports/football/leagues/nfl";

// MAIN
Startup(args, out weekOverride, out emailSettings, out players);
//GetNFLWeek(out year, out week, weekOverride, baseurl);

if (players.Count == 16 || players.Count == 32)
{
    Console.WriteLine($"year: {year} week: {week}\n");
    if (week > 0 && week < 19)
    {
        StringBuilder sb = Randomize(year, week, players, baseurl);
        SendEmail(emailSettings, players, week, sb);
    }
    else
    {
        Console.WriteLine($"Not in a regular season week.");
    }
}
else
{
    Console.WriteLine($"For equal distribution of byes there must be either 16 or 32 players. There are currently {players.Count} players.");
}

static void Startup(string[] args, out string weekOverride, out EmailSettings emailSettings, out List<PlayerInfo> players)
{
    try
    {
        var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
        var config = builder.Build();

        emailSettings = config.GetRequiredSection("EmailSettings").Get<EmailSettings>();

        players = config.GetRequiredSection("Players").Get<List<PlayerInfo>>();

        AppSettings appSettings = config.GetRequiredSection("AppSettings").Get<AppSettings>();
        weekOverride = appSettings.WeekOverride;
    }
    catch (Exception e)
    {
        weekOverride = "";
        emailSettings = new EmailSettings();
        players = new List<PlayerInfo>();
        Console.WriteLine(e.ToString());
    }
}

static void GetNFLWeek(out int year, out int week, string weekOverride, string baseurl)
{
    try
    {
        // DETERMINE NFL YEAR
        DateTime now = DateTime.Now;
        string seasonJson = RESTUtil.Get([], $"{baseurl}/seasons/{now.Year}/types/2");
        seasonJson = seasonJson.Replace("$ref", "reference");
        NFLSeason seasonResults = JsonSerializer.Deserialize<NFLSeason>(seasonJson);

        DateTime start = Convert.ToDateTime(seasonResults.startDate);
        DateTime end = Convert.ToDateTime(seasonResults.endDate);

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
            NFLObject weeksResults = JsonSerializer.Deserialize<NFLObject>(weeksJson);
            week = weeksResults.count;
        }
        else
        {
            bool success = Int32.TryParse(weekOverride, out week);
            {
                Console.WriteLine($"Could not convert '{weekOverride}' to a number.");
                week = 0;
            }
        }
    }
    catch (Exception ex)
    {
        year = DateTime.Now.Year;
        week = 0;
        Console.WriteLine(ex.ToString());
    }
}

static List<string> GetByeTeams(int nflYear, int week, string baseurl)
{
    string weekJson = RESTUtil.Get([], $"{baseurl}/seasons/{nflYear}/types/2/weeks/{week}");
    weekJson = weekJson.Replace("$ref", "reference");
    NFLWeekDetails weekInfo = JsonSerializer.Deserialize<NFLWeekDetails>(weekJson);

    List<string> byeTeams = [];
    if (weekInfo?.teamsOnBye != null)
    {
        foreach (var byeTeam in weekInfo.teamsOnBye)
        {
            string byeTeamJson = RESTUtil.Get([], byeTeam.reference);
            NFLTeam byeTeamResult = JsonSerializer.Deserialize<NFLTeam>(byeTeamJson);
            byeTeams.Add(byeTeamResult.nickname);
        }
    }

    return byeTeams;
}

static int GetByeTeamCountToDate (int nflYear, int week, string baseurl)
{
    int count = 0;
    for (int i = week - 1;  i > 0; i--)
    {
        count += GetByeTeams(nflYear, i, baseurl).Count;
    }

    return count;
}

static List<string> GetAllTeams(string baseurl)
{
    var allTeams = new List<String>();
    string teamsJson = RESTUtil.Get([], $"{baseurl}/teams?limit=32");
    teamsJson = teamsJson.Replace("$ref", "reference");
    NFLObject teamsResult = JsonSerializer.Deserialize<NFLObject>(teamsJson);
    foreach (var team in teamsResult.items)
    {
        string teamJson = RESTUtil.Get([], team.reference);
        NFLTeam teamResult = JsonSerializer.Deserialize<NFLTeam>(teamJson);
        allTeams.Add(teamResult.nickname);
    }

    return allTeams;
}

static StringBuilder Randomize(int year, int currentWeek, List<PlayerInfo> players, string baseurl)
{
    StringBuilder sb = new();
    try
    {
        var rnd = new Random();
        var allTeams = GetAllTeams(baseurl);
        var byeTeams = GetByeTeams(year, currentWeek, baseurl);
        byeTeams.ForEach(p => Console.WriteLine($"Bye Team: {p}"));

        //There are teams on bye so need to evenly distribute those teams
        if (byeTeams.Count != 0)
        {
            //Setup Lists and Queues
            Queue<string> randomNonByeTeams = new();
            var orderedPlayers = players.OrderBy(p => p.ID);

            var nonByeTeams = allTeams.Where(x => !byeTeams.Contains(x)).ToList();
            nonByeTeams.OrderBy(item => rnd.Next()).Distinct().ToList().ForEach(i => randomNonByeTeams.Enqueue(i));

            int byeMarker = GetByeTeamCountToDate(year, currentWeek, baseurl);
            Console.WriteLine($"Bye Team Count To Date: {byeMarker}\n");

            //Assign the new bye teams to the next player ids in consecutive order
            if (byeMarker >= 16 && players.Count <= 16)
            {
                byeMarker -= 16;
                Console.WriteLine($"Bye Team Marker Set To: {byeMarker}\n");
            }
            foreach (var byeTeamName in byeTeams)
            {
                byeMarker++;

                var player = orderedPlayers.Where(p => p.ID.Equals(byeMarker)).First();
                sb.AppendLine($"{player.Name}: {byeTeamName} & {randomNonByeTeams.Dequeue()}");

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
            Queue<string> randomTeams = new Queue<string>();
            allTeams.OrderBy(item => rnd.Next()).Distinct().ToList().ForEach(i => randomTeams.Enqueue(i));
            var randomPlayers = players.OrderBy(item => rnd.Next());

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
    }
    return sb;
}

static void SendEmail(EmailSettings emailSettings, List<PlayerInfo> players, int week, StringBuilder sb)
{
    var client = new SmtpClient(emailSettings.SMTPServer, emailSettings.SMTPPort)
    {
        Credentials = new NetworkCredential(emailSettings.FromEmail, emailSettings.Psswd),
        EnableSsl = true
    };
    try
    {
        client.Send(emailSettings.FromEmail, String.Join(",", players.Select(p => p.Email)), $"Week {week}", sb.ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not send emails. The following exception occurred:");
        Console.WriteLine(ex.ToString());
    }
}