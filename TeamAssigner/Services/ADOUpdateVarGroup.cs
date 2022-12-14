namespace TeamAssigner.Services
{
    using System.Collections.Specialized;
    using System.Text;
    using System.Text.Json;
    using TeamAssigner.Models;
    using TeamAssigner.Utils;

    internal class ADOUpdateVarGroup
    {
        static readonly string apiVer = "api-version=6.0-preview.1";
        NameValueCollection _authHeader;
        string _baseURL;
        string _varGroupID;
        public ADOUpdateVarGroup(string token, string baseURL, string varGroupID)
        {
            _authHeader = new NameValueCollection
            {
                { "Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($":{token}"))}" }
            };
            _baseURL = baseURL.Trim().TrimEnd('/');
            _varGroupID = varGroupID;
        }
        public string GetCurrentValueOfByeWeekMarker()
        {
            VariableGroupResults? results = GetDetails();
            return results?.variables?.ByeWeekMarker?.value ?? "0";
        }

        private VariableGroupResults? GetDetails()
        {
            RESTUtil restUtil = new RESTUtil();
            var varGroupGetURL = $"{_baseURL}/distributedtask/variablegroups?groupID={_varGroupID}&{apiVer}";
            Console.WriteLine($"Checking Bye Marker. Making GET call to:  {_baseURL}/distributedtask/variablegroups?groupID={_varGroupID}&{apiVer}");
            var json = restUtil.Get(_authHeader, varGroupGetURL);
            VariableGroupResults? results = JsonSerializer.Deserialize<VariableGroupResults>(json);
            return results;
        }

        public void SetNewValueOfByeWeekMarker(int newValue)
        {
            var varGroupPutURL = $"{_baseURL}/distributedtask/variablegroups/{_varGroupID}?{apiVer}";
            Console.WriteLine($"Getting Var Group existing values. Making GET call to: {_baseURL}/distributedtask/variablegroups/{_varGroupID}?{apiVer}");
            VariableGroupResults? results = GetDetails();
            results.variables.ByeWeekMarker.value = newValue.ToString();

            VariableGroupUpdate payload = new VariableGroupUpdate
            {
                id = _varGroupID,
                name = results.name,
                type = "Vsts",
                variables = results.variables
            };

            var json = JsonSerializer.Serialize(payload);
            Console.WriteLine($"Updating Bye Marker to {newValue.ToString()}. Making PUI call to: { _baseURL}/distributedtask/variablegroups/{_varGroupID}?{apiVer}");
            RESTUtil restUtil = new RESTUtil();
            string returnResults = restUtil.Put(_authHeader, varGroupPutURL, json);
        }
    }
}
