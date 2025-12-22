// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Security.Tests.Model;
using Xtensive.Orm.Security.Tests.Roles;

namespace Xtensive.Orm.Security.Tests
{
  [TestFixture]
  public class ImpersonationContextTests : SecurityTestBase
  {
    [Test]
    public void SessionImpersonateTest()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {

        var u = session.Query.All<Employee>().First();
        var ic = session.Impersonate(u);
        Assert.That(ic, Is.Not.Null);
        Assert.That(session.GetImpersonationContext(), Is.Not.Null);
        Assert.That(session.GetImpersonationContext(), Is.SameAs(ic));
        Assert.That(ic.Principal, Is.SameAs(u));

        ic.Undo();
        Assert.That(session.GetImpersonationContext(), Is.Null);

        // Should not fail in case multiple Undo
        ic.Undo();
        ic.Undo();
        // Should not fail if Dispose after Undo
        ic.Dispose();

        ic = session.Impersonate(u);
        ic.Dispose();
        Assert.That(session.GetImpersonationContext(), Is.Null);
        // Should not fail if Undo after Dispose
        ic.Undo();

        trx.Complete();
      }
    }

    [Test]
    public void NestedSessionImpersonationTest()
    {
      using (var s = Domain.OpenSession())
      using (var t = s.OpenTransaction()) {

        var users = s.Query.All<Employee>().ToList();
        var u1 = users[0];
        var u2 = users[1];
        var ic1 = s.Impersonate(u1);
        Assert.That(ic1, Is.Not.Null);
        Assert.That(s.GetImpersonationContext(), Is.Not.Null);
        Assert.That(s.GetImpersonationContext(), Is.SameAs(ic1));
        Assert.That(ic1.Principal, Is.SameAs(u1));

        var ic2 = s.Impersonate(u2);
        Assert.That(ic2, Is.Not.Null);
        Assert.That(s.GetImpersonationContext(), Is.Not.Null);
        Assert.That(s.GetImpersonationContext(), Is.SameAs(ic2));
        Assert.That(ic1, Is.Not.SameAs(ic2));
        Assert.That(ic2.Principal, Is.SameAs(u2));

        // Outer context can't be undone while is not active
        ic1.Undo();
        Assert.That(s.GetImpersonationContext(), Is.SameAs(ic2));

        ic2.Undo();
        // After outer context is undone, the nested one becomes outer
        Assert.That(s.GetImpersonationContext(), Is.SameAs(ic1));

        t.Complete();
      }
    }

    [Test]
    public void SecureQueryTest()
    {
      using (var s = Domain.OpenSession())
      using (var t = s.OpenTransaction()) {

        Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(3));

        var u1 = s.Query.All<Employee>().Single(u => u.Name == "SalesPerson");
        using (var ic = s.Impersonate(u1)) {

          Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(1));
          Assert.That(s.Query.All<VipCustomer>().Count(), Is.EqualTo(0));
          ic.Undo();
        }

        var u2 = s.Query.All<Employee>().Single(u => u.Name == "SalesManager");
        using (var ic = s.Impersonate(u2)) {

          Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(3));
          Assert.That(s.Query.All<VipCustomer>().Count(), Is.EqualTo(2));
          ic.Undo();
        }

        var u3 = s.Query.All<Employee>().Single(u => u.Name == "AutomobileManager");
        using (var ic = s.Impersonate(u3)) {

          Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(1));
          ic.Undo();
        }

        var u4 = s.Query.All<Employee>().Single(u => u.Name == "AircraftManager");
        using (var ic = s.Impersonate(u4)) {

          Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(1));
          ic.Undo();
        }

        // Merging 2 roles
        _ = u4.Roles.Add(s.Query.All<IRole>().OfType<AutomobileManagerRole>().Single());
        using (var ic = s.Impersonate(u4)) {

          Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(2));
          ic.Undo();
        }

        var u5 = s.Query.All<Employee>().Single(u => u.Name == "SouthBranchOfficeManager");
        using (var ic = s.Impersonate(u5)) {

          Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(2));
          ic.Undo();
        }

        var u6 = s.Query.All<Employee>().Single(u => u.Name == "NorthBranchOfficeManager");
        using (var ic = s.Impersonate(u6)) {

          Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(1));
          ic.Undo();
        }

        var u7 = s.Query.All<Employee>().Single(u => u.Name == "AllBranchOfficeManager");
        using (var ic = s.Impersonate(u7)) {

          Assert.That(s.Query.All<Customer>().Count(), Is.EqualTo(3));
          ic.Undo();
        }

        t.Complete();
      }
    }
  }
}