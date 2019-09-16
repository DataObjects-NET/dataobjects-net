// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using System.Linq;
using Xtensive.Orm;
using Xtensive.Orm.Security.Tests.Model;
using Xtensive.Orm.Security.Tests.Permissions;

namespace Xtensive.Orm.Security.Tests.Roles
{
  public class SalesPersonRole : EmployeeRole
  {
    private static IQueryable<Customer> GetCustomers(ImpersonationContext context, QueryEndpoint query)
    {
      return query.All<Customer>()
        .Where(c => c.IsVip == false);
    }

    protected override void RegisterPermissions()
    {
      RegisterPermission(new CustomerPermission(true, GetCustomers));
    }

    public SalesPersonRole(Session session)
      : base(session)
    {
    }
  }
}