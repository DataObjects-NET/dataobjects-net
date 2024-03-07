// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Security.Tests.Model;
using Xtensive.Orm.Security.Tests.Permissions;
using Xtensive.Orm.Security.Tests.Roles;

namespace Xtensive.Orm.Security.Tests
{
  [TestFixture]
  public class PermissionSetTests : SecurityTestBase
  {
    [Test]
    public void SalesManagerRoleTest()
    {
      using (var s = Domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        var role = new SalesManagerRole(s);
        var roles = new List<Role> { role };
        var permissions = new PermissionSet(roles);

        Assert.AreEqual(role.Permissions.Count, permissions.Count);
        Assert.IsTrue(permissions.Contains<Permission<Customer>>(p => p.CanRead));
        Assert.IsTrue(permissions.Contains<Permission<Customer>>(p => p.CanWrite));

        Assert.IsTrue(permissions.Contains<CustomerPermission>(p => p.CanRead));
        Assert.IsTrue(permissions.Contains<CustomerPermission>(p => p.CanWrite));

        Assert.IsTrue(permissions.Contains<VipCustomerPermission>(p => p.CanRead));
        Assert.IsTrue(permissions.Contains<VipCustomerPermission>(p => p.CanWrite));
        Assert.IsTrue(permissions.Contains<VipCustomerPermission>(p => p.CanDiscount));
      }
    }

    [Test]
    public void SalesPersonRoleTest()
    {
      using (var s = Domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        var role = new SalesPersonRole(s);
        var roles = new List<Role> { role };
        var permissions = new PermissionSet(roles);

        Assert.AreEqual(role.Permissions.Count, permissions.Count);
        Assert.IsTrue(permissions.Contains<Permission<Customer>>(p => p.CanRead));
        Assert.IsTrue(permissions.Contains<Permission<Customer>>(p => p.CanWrite));

        Assert.IsTrue(permissions.Contains<CustomerPermission>(p => p.CanRead));
        Assert.IsTrue(permissions.Contains<CustomerPermission>(p => p.CanWrite));

        Assert.IsFalse(permissions.Contains<VipCustomerPermission>(p => p.CanRead));
        Assert.IsFalse(permissions.Contains<VipCustomerPermission>(p => p.CanWrite));
        Assert.IsFalse(permissions.Contains<VipCustomerPermission>(p => p.CanDiscount));
      }
    }
  }
}