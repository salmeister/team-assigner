namespace TeamAssigner.Models
{
    using System.Text.Json.Serialization;

    public class AppSettingsADOBaseURL
    {
        public string value { get; set; }
    }

    public class AppSettingsToken
    {
        public object value { get; set; }
        public bool isSecret { get; set; }
    }

    public class AppSettingsVariableGroupIDToModify
    {
        public string value { get; set; }
    }

    public class ByeWeekMarker
    {
        public string value { get; set; }
    }

    public class CreatedBy
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
    }

    public class EmailSettingsFromEmail
    {
        public string value { get; set; }
    }

    public class EmailSettingsPsswd
    {
        public object value { get; set; }
        public bool isSecret { get; set; }
    }

    public class ModifiedBy
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
    }

    public class ProjectReference
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class VariableGroupResults
    {
        public Variables variables { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public CreatedBy createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public ModifiedBy modifiedBy { get; set; }
        public DateTime modifiedOn { get; set; }
        public bool isShared { get; set; }
        public List<VariableGroupProjectReference> variableGroupProjectReferences { get; set; }
    }

    public class VariableGroupProjectReference
    {
        public ProjectReference projectReference { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Variables
    {
        public ByeWeekMarker ByeWeekMarker { get; set; }
    }


}
