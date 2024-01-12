
namespace TeamAssigner.Services
{
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using TeamAssigner.Models;

    public sealed class GoogleSheetsService
    {
        readonly string sheetID;
        readonly string range;
        readonly string keyFileName;

        public GoogleSheetsService(string sheetID, string range, string keyFileName)
        {
            this.sheetID = sheetID;
            this.range= range;
            this.keyFileName = keyFileName;
        }

        public List<PlayerInfo> ReadDoc()
        {

            GoogleCredential credential;
            using (var stream = new FileStream(keyFileName, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(new string[] { SheetsService.Scope.Spreadsheets });
            }

            var sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ShoppingList"
            });

            SpreadsheetsResource.ValuesResource.GetRequest getRequest = sheetsService.Spreadsheets.Values.Get(sheetID, range);

            var getResponse = getRequest.Execute();
            IList<IList<Object>> values = getResponse.Values;

            List<PlayerInfo> players = [];
            if (values != null && values.Count > 0)
            {
                for (int i = 1; i < values.Count; i++)
                {
                    players.Add(new PlayerInfo()
                    {
                        ID = i-1,
                        Name = values[i][0].ToString(),
                        Email = values[i][1].ToString()
                    });
                }
                Console.WriteLine("Players:");
                players.ForEach(p => Console.WriteLine($"{p.ID}\t{p.Name}\t{p.Email}"));
            }

            return players;
        }
    }
}
