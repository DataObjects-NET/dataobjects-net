namespace Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel
{
    public interface IEmployee : IPerson
    {
        [Field]
        double EmploymentFraction { get; set; }

        [Field]
        string PensionNumber { get; set; }

        [Field]
        string InsuranceNumber { get; set; }
    }
}