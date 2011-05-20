using System;

namespace Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel
{
  public class Employee : Record, IEmployee
  {
    [Field]
    public String InsuranceNumber { get; set; }

    [Field]
    public String PensionNumber { get; set; }

    [Field]
    public Double EmploymentFraction { get; set; }

    [Field]
    public String PersonNumber { get; set; }

    [Field]
    public String LastName { get; set; }

    [Field]
    public String FirstName { get; set; }

    [Field]
    public Boolean Active { get; set; }

    [Field]
    public String BankAccountNumber { get; set; }

    [Field]
    public String Notes { get; set; }

    [Field]
    public String WebSite { get; set; }

    [Field]
    public String Email { get; set; }

    [Field]
    public String Fax { get; set; }

    [Field]
    public String Mobile { get; set; }

    [Field]
    public String Phone { get; set; }

    [Field]
    public ICountry Country { get; set; }

    [Field]
    public String Place { get; set; }

    [Field]
    public String PostalCode { get; set; }

    [Field]
    public String Address { get; set; }

    [Field]
    public Int32 Number { get; set; }

    [Field]
    public String Name { get; set; }

    public Employee(IParty createdBy)
      : base(createdBy)
    {
      try {
        base.Initialize(typeof (Employee));
      }
      catch (Exception ex) {
        base.InitializationError(typeof (Employee), ex);
        throw;
      }
    }
  }


  public class Country : Record, ICountry
  {
    [Field]
    public String Name { get; set; }

    public Country(IParty createdBy)
      : base(createdBy)
    {
      try {
        base.Initialize(typeof (Country));
      }
      catch (Exception ex) {
        base.InitializationError(typeof (Country), ex);
        throw;
      }
    }
  }
}