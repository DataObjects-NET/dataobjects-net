using System.Configuration;
using NUnit.Framework;
using System;
using TestCommon;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Security.Tests.Model;
using Xtensive.Orm.Security.Tests.Roles;
using Xtensive.Reflection;

namespace Xtensive.Orm.Security.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    protected Domain Domain { get; private set; }

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
      var config = BuildConfiguration();
      Domain = BuildDomain(config);
      PopulateData();
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
      Domain.DisposeSafely();
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (Permission).Assembly);
      configuration.Types.Register(typeof (AutoBuildTest).Assembly);
      return configuration;
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      try {
        return Domain.Build(configuration);
      }
      catch (Exception e) {
        Console.WriteLine(GetType().GetFullName());
        Console.WriteLine(e);
        throw;
      }
    }

    protected virtual void PopulateData()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          // Branches
          var southBranch = new Branch(session) { Name = "South"};
          var northBranch = new Branch(session) { Name = "North"};
          
          // Customers
          new Customer(session) {IsAutomobileIndustry = true, Branch = southBranch};
          new VipCustomer(session) {IsAircraftIndustry = true, Reason = "High sales", Branch = southBranch};
          new VipCustomer(session) {Reason = "Relative", Branch = northBranch};

          // Roles
          var salesPersonRole = new SalesPersonRole(session);
          var salesManagerRole = new SalesManagerRole(session);
          var automobileManagerRole = new AutomobileManagerRole(session);
          var aircraftManagerRole = new AircraftManagerRole(session);
          var southBranchOfficeManager =  new BranchOfficeManagerRole(session, southBranch);
          var northBranchOfficeManager =  new BranchOfficeManagerRole(session, northBranch);

          // Employees
          var u1 = new Employee(session);
          u1.Roles.Add(salesPersonRole);
          u1.Name = "SalesPerson";

          var u2 = new Employee(session);
          u2.Roles.Add(salesManagerRole);
          u2.Name = "SalesManager";

          var u3 = new Employee(session);
          u3.Roles.Add(automobileManagerRole);
          u3.Name = "AutomobileManager";

          var u4 = new Employee(session);
          u4.Roles.Add(aircraftManagerRole);
          u4.Name = "AircraftManager";

          var u5 = new Employee(session);
          u5.Roles.Add(southBranchOfficeManager);
          u5.Name = "SouthBranchOfficeManager";

          var u6 = new Employee(session);
          u6.Roles.Add(northBranchOfficeManager);
          u6.Name = "NorthBranchOfficeManager";

          var u7 = new Employee(session);
          u7.Roles.Add(southBranchOfficeManager);
          u7.Roles.Add(northBranchOfficeManager);
          u7.Name = "AllBranchOfficeManager";
          t.Complete();
        }
      }
    }
  }
}
