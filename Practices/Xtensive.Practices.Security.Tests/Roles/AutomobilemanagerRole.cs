// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using System.Linq;
using Xtensive.Orm;
using Xtensive.Practices.Security.Tests.Model;
using Xtensive.Practices.Security.Tests.Permissions;

namespace Xtensive.Practices.Security.Tests.Roles
{
  public class AutomobileManagerRole : EmployeeRole
  {
    private static IQueryable<Customer> GetCustomers(ImpersonationContext context, QueryEndpoint query)
    {
      return query.All<Customer>()
        .Where(c => c.IsAutomobileIndustry);
    }

    protected override void RegisterPermissions()
    {
      RegisterPermission(new CustomerPermission(true, GetCustomers));
    }

    public AutomobileManagerRole(Session session)
      : base(session)
    {
    }
  }
}