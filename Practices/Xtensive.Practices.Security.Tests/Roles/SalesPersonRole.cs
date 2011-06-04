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
  public class SalesPersonRole : Role
  {
    private static IQueryable<Customer> GetCustomers(ImpersonationContext context, QueryEndpoint query)
    {
      return query.All<Customer>()
        .Where(c => c.IsVip == false);
    }

    public SalesPersonRole()
    {
      RegisterPermission(new CustomerPermission(true, GetCustomers));
    }
  }
}