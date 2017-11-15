using System;

namespace Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel
{
  public class Employee : Record, IEmployee
  {
    [Field(Length = 200)]
    public String InsuranceNumber { get; set; }

    [Field(Length = 200)]
    public String PensionNumber { get; set; }

    [Field]
    public Double EmploymentFraction { get; set; }

    [Field(Length = 200)]
    public String PersonNumber { get; set; }

    [Field(Length = 200)]
    public String LastName { get; set; }

    [Field(Length = 200)]
    public String FirstName { get; set; }

    [Field]
    public Boolean Active { get; set; }

    [Field(Length = 200)]
    public String BankAccountNumber { get; set; }

    [Field(Length = 200)]
    public String Notes { get; set; }

    [Field(Length = 200)]
    public String WebSite { get; set; }

    [Field(Length = 200)]
    public String Email { get; set; }

    [Field(Length = 200)]
    public String Fax { get; set; }

    [Field(Length = 200)]
    public String Mobile { get; set; }

    [Field(Length = 200)]
    public String Phone { get; set; }

    [Field]
    public ICountry Country { get; set; }

    [Field(Length = 200)]
    public String Place { get; set; }

    [Field(Length = 200)]
    public String PostalCode { get; set; }

    [Field(Length = 200)]
    public String Address { get; set; }

    [Field]
    public Int32 Number { get; set; }

    [Field(Length = 200)]
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

    [Field(Length = 3)]
    public string Code { get; set; }

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