using System.Configuration;
using NUnit.Framework;
using TestCommon;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Security.Tests.Model;
using Xtensive.Orm.Security.Tests.Roles;
using Xtensive.Reflection;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Security.Tests
{
  [TestFixture]
  public abstract class SecurityTestBase : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Permission).Assembly);
      configuration.Types.Register(typeof(SecurityTestBase).Assembly);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {

        // Branches
        var southBranch = new Branch(session) { Name = "South" };
        var northBranch = new Branch(session) { Name = "North" };

        // Customers
        _ = new Customer(session) { IsAutomobileIndustry = true, Branch = southBranch };
        _ = new VipCustomer(session) { IsAircraftIndustry = true, Reason = "High sales", Branch = southBranch };
        _ = new VipCustomer(session) { Reason = "Relative", Branch = northBranch };

        // Roles
        var salesPersonRole = new SalesPersonRole(session);
        var salesManagerRole = new SalesManagerRole(session);
        var automobileManagerRole = new AutomobileManagerRole(session);
        var aircraftManagerRole = new AircraftManagerRole(session);
        var southBranchOfficeManager = new BranchOfficeManagerRole(session, southBranch);
        var northBranchOfficeManager = new BranchOfficeManagerRole(session, northBranch);

        // Employees
        var u1 = new Employee(session);
        _ = u1.Roles.Add(salesPersonRole);
        u1.Name = "SalesPerson";

        var u2 = new Employee(session);
        _ = u2.Roles.Add(salesManagerRole);
        u2.Name = "SalesManager";

        var u3 = new Employee(session);
        _ = u3.Roles.Add(automobileManagerRole);
        u3.Name = "AutomobileManager";

        var u4 = new Employee(session);
        _ = u4.Roles.Add(aircraftManagerRole);
        u4.Name = "AircraftManager";

        var u5 = new Employee(session);
        _ = u5.Roles.Add(southBranchOfficeManager);
        u5.Name = "SouthBranchOfficeManager";

        var u6 = new Employee(session);
        _ = u6.Roles.Add(northBranchOfficeManager);
        u6.Name = "NorthBranchOfficeManager";

        var u7 = new Employee(session);
        _ = u7.Roles.Add(southBranchOfficeManager);
        _ = u7.Roles.Add(northBranchOfficeManager);
        u7.Name = "AllBranchOfficeManager";
        t.Complete();
      }
    }
  }
}
