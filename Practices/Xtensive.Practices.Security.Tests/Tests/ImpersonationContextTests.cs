// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Practices.Security.Tests.Model;
using Xtensive.Practices.Security.Tests.Roles;

namespace Xtensive.Practices.Security.Tests
{
  [TestFixture]
  public class ImpersonationContextTests : AutoBuildTest
  {
    [Test]
    public void SessionImpersonateTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var trx = session.OpenTransaction()) {
          
          var u = session.Query.All<Employee>().First();
          var ic = session.Impersonate(u);
          Assert.IsNotNull(ic);
          Assert.IsNotNull(session.GetImpersonationContext());
          Assert.AreSame(ic, session.GetImpersonationContext());
          Assert.AreSame(u, ic.Principal);

          ic.Undo();
          Assert.IsNull(session.GetImpersonationContext());

          // Should not fail in case multiple Undo
          ic.Undo();
          ic.Undo();
          // Should not fail if Dispose after Undo
          ic.Dispose();

          ic = session.Impersonate(u);
          ic.Dispose();
          Assert.IsNull(session.GetImpersonationContext());
          // Should not fail if Undo after Dispose
          ic.Undo();

          trx.Complete();
        }
      }
    }

    [Test]
    public void NestedSessionImpersonationTest()
    {
      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {
          
          var users = s.Query.All<Employee>().ToList();
          var u1 = users[0];
          var u2 = users[1];
          var ic1 = s.Impersonate(u1);
          Assert.IsNotNull(ic1);
          Assert.IsNotNull(s.GetImpersonationContext());
          Assert.AreSame(ic1, s.GetImpersonationContext());
          Assert.AreSame(u1, ic1.Principal);

          var ic2 = s.Impersonate(u2);
          Assert.IsNotNull(ic2);
          Assert.IsNotNull(s.GetImpersonationContext());
          Assert.AreSame(ic2, s.GetImpersonationContext());
          Assert.AreNotSame(ic1, ic2);
          Assert.AreSame(u2, ic2.Principal);

          // Outer context can't be undone while is not active
          ic1.Undo();
          Assert.AreSame(ic2, s.GetImpersonationContext());

          ic2.Undo();
          // After outer context is undone, the nested one becomes outer
          Assert.AreSame(ic1, s.GetImpersonationContext());

          t.Complete();
        }
      }
    }

    [Test]
    public void SecureQueryTest()
    {
      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {

          Assert.AreEqual(3, s.Query.All<Customer>().Count());

          var u1 = s.Query.All<Employee>().Single(u => u.Name == "SalesPerson");
          using (var ic = s.Impersonate(u1)) {
            
            Assert.AreEqual(1, s.Query.All<Customer>().Count());
            Assert.AreEqual(0, s.Query.All<VipCustomer>().Count());
            ic.Undo();
          }

          var u2 = s.Query.All<Employee>().Single(u => u.Name == "SalesManager");
          using (var ic = s.Impersonate(u2)) {
            
            Assert.AreEqual(3, s.Query.All<Customer>().Count());
            Assert.AreEqual(2, s.Query.All<VipCustomer>().Count());
            ic.Undo();
          }

          var u3 = s.Query.All<Employee>().Single(u => u.Name == "AutomobileManager");
          using (var ic = s.Impersonate(u3)) {
            
            Assert.AreEqual(1, s.Query.All<Customer>().Count());
            ic.Undo();
          }

          var u4 = s.Query.All<Employee>().Single(u => u.Name == "AircraftManager");
          using (var ic = s.Impersonate(u4)) {
            
            Assert.AreEqual(1, s.Query.All<Customer>().Count());
            ic.Undo();
          }

          // Merging 2 roles
          u4.PrincipalRoles.Add(new AutomobileManagerRole());
          using (var ic = s.Impersonate(u4)) {
            
            Assert.AreEqual(2, s.Query.All<Customer>().Count());
            ic.Undo();
          }

          t.Complete();
        }
      }
    }

  }
}