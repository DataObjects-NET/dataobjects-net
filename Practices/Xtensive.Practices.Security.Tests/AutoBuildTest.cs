using System.Configuration;
using NUnit.Framework;
using System;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Practices.Security.Tests.Model;
using Xtensive.Practices.Security.Tests.Roles;
using Xtensive.Reflection;

namespace Xtensive.Practices.Security.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    protected Domain Domain { get; private set; }

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      var config = BuildConfiguration();
      Domain = BuildDomain(config);
      PopulateData();
    }

    [TestFixtureTearDown]
    public virtual void TestFixtureTearDown()
    {
      Domain.DisposeSafely();
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      return DomainConfiguration.Load("Default");
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      try
      {
        return Domain.Build(configuration);
      }
      catch (Exception e)
      {
        Log.Error(GetType().GetFullName());
        Log.Error(e);
        throw;
      }
    }

    protected virtual void PopulateData()
    {
      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {
          
          new Customer(s) {IsAutomobileIndustry = true};
          new VipCustomer(s) {IsAircraftIndustry = true};

          var u1 = new Employee(s);
          u1.PrincipalRoles.Add(new SalesPersonRole());
          u1.Name = "SalesPerson";

          var u2 = new Employee(s);
          u2.PrincipalRoles.Add(new SalesManagerRole());
          u2.Name = "SalesManager";

          var u3 = new Employee(s);
          u3.PrincipalRoles.Add(new AutomobileManagerRole());
          u3.Name = "AutomobileManager";

          var u4 = new Employee(s);
          u4.PrincipalRoles.Add(new AircraftManagerRole());
          u4.Name = "AircraftManager";
          t.Complete();
        }
      }
    }
  }
}
