
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using TeamAssigner.Models;
using TeamAssigner.Services;
using TeamAssigner.Utils;
using System.Collections.Specialized;
using System.Text.Json;

AppSettings appSettings;
EmailSettings emailSettings;
List<TeamInfo> teams;
List<PlayerInfo> players;
int week;

Startup(args, out appSettings, out emailSettings, out teams, out players);
GetNFLWeek(out week, appSettings.NFLSeason, appSettings.WeekOverride);

if (week < 19)
{
    StringBuilder sb = Randomize(appSettings, teams, players, week);
    SendEmail(emailSettings, players, week, sb);
}

static void Startup(string[] args, out AppSettings appSettings, out EmailSettings emailSettings, out List<TeamInfo> teams, out List<PlayerInfo> players)
{
    var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
    var config = builder.Build();

    emailSettings = config.GetRequiredSection("EmailSettings").Get<EmailSettings>();
    appSettings = config.GetRequiredSection("AppSettings").Get<AppSettings>();
    teams = config.GetRequiredSection("Teams").Get<List<TeamInfo>>();
    players = config.GetRequiredSection("Players").Get<List<PlayerInfo>>();
}

static void GetNFLWeek(out int week, string nflYear, string weekOverride)
{

    if (String.IsNullOrWhiteSpace(weekOverride))
    {
        RESTUtil restUtil = new RESTUtil();
        string returnResults = restUtil.Get(new NameValueCollection(), $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{nflYear}/types/2/weeks");
        returnResults = returnResults.Replace("$ref", "reference");
        NFLWeeks results = JsonSerializer.Deserialize<NFLWeeks>(returnResults);
        string weekURL = results.items.Last().reference;
        string weekJson = restUtil.Get(new NameValueCollection(), weekURL);
        NFLWeekDetails weekInfo = JsonSerializer.Deserialize<NFLWeekDetails>(weekJson);
        week = weekInfo.number;
    }
    else
    {
        _ = Int32.TryParse(weekOverride, out week);
    }

    Console.WriteLine($"Week: {week}\n");
}

static StringBuilder Randomize(AppSettings appSettings, List<TeamInfo> teams, List<PlayerInfo> players, int currentWeek)
{
    StringBuilder sb = new StringBuilder();
    var rnd = new Random();
    var byeTeams = teams.Where(t => t.ByeWeek.Equals(currentWeek)).Select(t => t.Name);
    if (byeTeams.Any())
    {
        //There are teams on bye so need to evenly distribute those teams
        
        //Setup Lists and Queues
        Queue<string> randomNonByeTeams = new Queue<string>();
        teams.Where(t => t.ByeWeek != currentWeek).OrderBy(item => rnd.Next()).Distinct().ToList().ForEach(i => randomNonByeTeams.Enqueue(i.Name));
        var orderedPlayers = players.OrderBy(p => p.ID);

        //Read the variable that sets that last team id (from last week) that received a bye team
        ADOUpdateVarGroup adoUpdateVarGroup = new ADOUpdateVarGroup(appSettings.Token, appSettings.ADOBaseURL, appSettings.VariableGroupIDToModify);
        int.TryParse(adoUpdateVarGroup.GetCurrentValueOfByeWeekMarker(), out int nextPlayerToGetBye);       

        //Assign the new bye teams to the next player ids in consecutive order
        foreach (var byeTeamName in byeTeams)
        {
            if (nextPlayerToGetBye >= 16)
            {
                nextPlayerToGetBye = 0;
            }
            nextPlayerToGetBye++;

            var player = orderedPlayers.Where(p => p.ID.Equals(nextPlayerToGetBye)).First();
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

        //Update the variable to the highest team id that received a bye team
        adoUpdateVarGroup.SetNewValueOfByeWeekMarker(nextPlayerToGetBye);
    }
    else
    {
        //No bye teams so evenly distribute all teams
        Queue<string> randomTeams = new Queue<string>();
        teams.OrderBy(item => rnd.Next()).Distinct().ToList().ForEach(i => randomTeams.Enqueue(i.Name));
        var randomPlayers = players.OrderBy(item => rnd.Next());

        foreach (var player in randomPlayers)
        {
            sb.AppendLine($"{player.Name}: {randomTeams.Dequeue()} & {randomTeams.Dequeue()}");
        }
    } 

    Console.WriteLine(sb.ToString());
    return sb;
}

static void SendEmail(EmailSettings emailSettings, List<PlayerInfo> players, int week, StringBuilder sb)
{
    var client = new SmtpClient(emailSettings.SMTPServer, emailSettings.SMTPPort)
    {
        Credentials = new NetworkCredential(emailSettings.FromEmail, emailSettings.Psswd),
        EnableSsl = true
    };
    client.Send(emailSettings.FromEmail, String.Join(",", players.Select(p => p.Email)), $"Week {week}", sb.ToString());
}
