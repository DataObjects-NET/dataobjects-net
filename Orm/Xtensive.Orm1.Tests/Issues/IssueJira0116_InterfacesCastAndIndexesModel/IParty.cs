namespace Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel
{
    public interface IParty : IRecord
    {
        /// <summary>
        /// Name of Party
        /// For a company it would be company name
        /// For an employee it would be employee name
        /// </summary>
        [Field]//(Length = 512, Indexed = true, Nullable = false)]
        string Name { get; set; }
        
        [Field]
        int Number { get; set; }

        [Field]
        string Address { get; set; }

        [Field]
        string PostalCode { get; set; }

        [Field]
        string Place { get; set; }

        [Field]
        ICountry Country { get; set; }

        [Field]
        string Phone { get; set; }

        [Field]
        string Mobile { get; set; }

        [Field]
        string Fax { get; set; }

        [Field]
        string Email { get; set; }

        [Field]
        string WebSite { get; set; }

        [Field]
        string Notes { get; set; }

        [Field]
        string BankAccountNumber { get; set; }

        [Field]
        bool Active { get; set; }
    }
}