// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.26

using System;
using System.Linq;
using Xtensive.Orm;
using Xtensive.Practices.Security.Tests.Model;

namespace Xtensive.Practices.Security.Tests.Roles
{
  public class BranchOfficeManagerRole : EmployeeRole
  {
    [Field]
    public Branch Branch { get; set; }

    private IQueryable<Customer> GetCustomers(ImpersonationContext context, QueryEndpoint query)
    {
      return query.All<Customer>()
        .Where(c => c.Branch == Branch);
    }

    protected override void RegisterPermissions()
    {
      RegisterPermission(new Permission<Customer>(true, GetCustomers));
    }

    public BranchOfficeManagerRole(Session session, Branch branch)
      : base(session)
    {
      Branch = branch;
      Name = branch.Name + "OfficeManager";
    }
  }
}