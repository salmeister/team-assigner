
using Microsoft.Extensions.Configuration;
using TeamAssigner.Models;
using TeamAssigner.Services;


var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
var config = builder.Build();

List<PlayerInfo> players = config.GetRequiredSection("Players").Get<List<PlayerInfo>>();

AppSettings appSettings = config.GetRequiredSection("AppSettings").Get<AppSettings>();
string weekOverride = appSettings.WeekOverride;
string baseurl = appSettings.baseAPIURL;

EmailSettings emailSettings = config.GetRequiredSection("EmailSettings").Get<EmailSettings>();
string adminEmail = emailSettings.FromEmail;
EmailService emailService = new(emailSettings.SMTPServer, emailSettings.SMTPPort, emailSettings.FromEmail, emailSettings.Psswd);

TeamRandomizer teamRandomizer = new(players, emailService, adminEmail, baseurl, weekOverride);
teamRandomizer.Run();