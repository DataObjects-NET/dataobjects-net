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
  public class SalesManagerRole : SalesPersonRole
  {
    private static IQueryable<Customer> GetCustomers(ImpersonationContext context, QueryEndpoint query)
    {
      return query.All<Customer>();
    }

    private static IQueryable<VipCustomer> GetVipCustomers(ImpersonationContext context, QueryEndpoint query)
    {
      return query.All<VipCustomer>().Where(v => v.Reason != "Relative");
    }

    protected override void OnInitialize()
    {
      base.OnInitialize();
      RegisterPermission(new CustomerPermission(true, GetCustomers));
      RegisterPermission(new VipCustomerPermission(GetVipCustomers));
    }

    public SalesManagerRole(Session session)
      : base(session)
    {
    }
  }
}