

using ilsFramework.Core.SQLite4Unity3d;

public class InputModifierInfo
{
    [PrimaryKey]
    public string GUID { get; set; }
        
    public string ModifierJson { get; set; }
}