namespace Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel
{
    public interface IPerson : IParty
    {
        [Field]
        string FirstName { get; set; }

        [Field]
        string LastName { get; set; }

        [Field]
        string PersonNumber { get; set; }
    }
}