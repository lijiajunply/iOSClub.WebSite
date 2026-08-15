namespace iOSClub.Data.DataObjects;

public abstract class DataObject
{
    public override string ToString() => $"{GetType()} : {DataTool.GetProperties(this)}; Guid: {Guid.NewGuid():N}";
    public string GetHashKey() => DataTool.ToMd5Hash(ToString());
}
