
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Security;

var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);

CultureInfo myCI = new CultureInfo("en-US");
Calendar myCal = myCI.Calendar;
CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
DayOfWeek myFirstDOW = DayOfWeek.Tuesday;
int week = myCal.GetWeekOfYear(DateTime.Now, myCWR, myFirstDOW) - 36;
Console.WriteLine($"Week: {week}\n");

if (week < 18)
{

    var config = builder.Build();
    Dictionary<string, string> players = config.GetSection("Players").GetChildren().ToDictionary(x => x.Key, x => x.Value);
    string fromEmail = config.GetSection("Email")["fromEmail"];
    string psswd = config.GetSection("Email")["psswd"];

    List<string> teams = new List<string>()
    {
        "Cardinals",
        "Falcons",
        "Ravens",
        "Bills",
        "Panthers",
        "Bears",
        "Bengals",
        "Browns",
        "Cowboys",
        "Broncos",
        "Lions",
        "Packers",
        "Texans",
        "Colts",
        "Jaguars",
        "Chiefs",
        "Chargers",
        "Rams",
        "Dolphins",
        "Vikings",
        "Patriots",
        "Saints",
        "Giants",
        "Jets",
        "Raiders",
        "Eagles",
        "Steelers",
        "49ers",
        "Seahawks",
        "Buccaneers",
        "Titans",
        "Commanders"
    };

    var rnd = new Random();
    var randomTeam = teams.OrderBy(item => rnd.Next()).Distinct().ToList();
    var randomPlayers = players.Keys.OrderBy(item => rnd.Next());

    int i = 0;
    StringBuilder sb = new StringBuilder();
    foreach (var player in randomPlayers)
    {
        sb.AppendLine($"{player}: {randomTeam.ElementAt(i)} & {randomTeam.ElementAt(i + 1)}");
        i += 2;
    }

    var client = new SmtpClient("smtp.gmail.com", 587)
    {
        Credentials = new NetworkCredential(fromEmail, psswd),
        EnableSsl = true
    };
    Console.WriteLine(sb.ToString());
    client.Send(fromEmail, String.Join(",", players.Values), $"Week {week}", sb.ToString());
}

