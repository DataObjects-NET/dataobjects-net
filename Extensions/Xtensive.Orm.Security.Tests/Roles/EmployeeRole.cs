// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.26

using Xtensive.Orm;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Security.Tests.Roles
{
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public abstract class EmployeeRole : Role
  {
    [Field, Key]
    public int Id { get; private set; }

    protected EmployeeRole(Session session)
      : base(session)
    {}
  }
}