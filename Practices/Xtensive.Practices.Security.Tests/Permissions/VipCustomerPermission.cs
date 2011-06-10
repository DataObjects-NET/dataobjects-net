// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using System.Linq;
using Xtensive.Orm;
using Xtensive.Practices.Security.Tests.Model;

namespace Xtensive.Practices.Security.Tests.Permissions
{
  public class VipCustomerPermission : Permission<VipCustomer>
  {
    public bool CanDiscount { get; private set; }

    public VipCustomerPermission(Func<ImpersonationContext, QueryEndpoint, IQueryable<VipCustomer>> query)
      : base(true, query)
    {
      CanDiscount = true;
    }
  }
}