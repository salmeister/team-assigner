
using Microsoft.Extensions.Configuration;
using TeamAssigner.Models;
using TeamAssigner.Services;

// Load appsettings.json, creds.json, and quotes.json from the publish directory
// (beside TeamAssigner.dll), not from the process's original working directory.
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
var config = builder.Build();

AppSettings appSettings = config.GetRequiredSection("AppSettings").Get<AppSettings>();
string weekOverride = appSettings.WeekOverride;
string baseurl = appSettings.BaseAPIURL;
string scoresBaseURL = appSettings.ScoresBaseAPIURL;
string quoteurl = appSettings.QuoteAPIURL;
string keyFileName = appSettings.KeyFileName;
string sheetID = appSettings.SheetID;
string sheetRange = appSettings.SheetRange;

EmailSettings emailSettings = config.GetRequiredSection("EmailSettings").Get<EmailSettings>();
string adminEmail = emailSettings.FromEmail;
EmailService emailService = new(emailSettings.SMTPServer, emailSettings.SMTPPort, emailSettings.FromEmail, emailSettings.Psswd);

GoogleSheetsService gss = new(sheetID, sheetRange, keyFileName);
IList<PlayerInfo> players = gss.ReadDoc();

if (players.Count > 0)
{
    TeamRandomizer teamRandomizer = new(players, emailService, adminEmail, baseurl, scoresBaseURL, quoteurl, weekOverride);
    teamRandomizer.Run();
}
else
{
    string msg = "There was an error retreiving the player emails.";
    Console.WriteLine(msg);
    emailService?.SendEmail(adminEmail, "Error Running NFL Team Assigner", $"{msg}");
}
