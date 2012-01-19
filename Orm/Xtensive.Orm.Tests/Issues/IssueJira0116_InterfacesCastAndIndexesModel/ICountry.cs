namespace Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel
{
    public interface ICountry : IRecord
    {
        [Field(Length = 512)]
        string Name{ get; set;}
    }
}