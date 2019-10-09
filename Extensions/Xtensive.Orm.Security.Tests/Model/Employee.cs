// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using Xtensive.Orm;

namespace Xtensive.Orm.Security.Tests.Model
{
  [HierarchyRoot]
  public class Employee : GenericPrincipal
  {
    [Field, Key]
    public int Id { get; private set; }

    public Employee(Session session)
      : base(session)
    {}
  }
}