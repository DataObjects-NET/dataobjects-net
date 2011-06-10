// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using NUnit.Framework;
using Xtensive.Practices.Security.Tests.Model;
using Xtensive.Practices.Security.Tests.Permissions;
using Xtensive.Practices.Security.Tests.Roles;
using System.Collections.Generic;

namespace Xtensive.Practices.Security.Tests
{
  [TestFixture]
  public class PermissionSetTests
  {
    [Test]
    public void SalesPersonRoleTest()
    {
      var role = new SalesPersonRole();
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

    [Test]
    public void SalesManagerRoleTest()
    {
      var role = new SalesManagerRole();
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
}