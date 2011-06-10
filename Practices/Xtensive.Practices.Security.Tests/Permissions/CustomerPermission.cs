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
  public class CustomerPermission : Permission<Customer>
  {
    public CustomerPermission(bool canWrite, Func<ImpersonationContext, QueryEndpoint, IQueryable<Customer>> query) : base(canWrite, query)
    {}
  }
}