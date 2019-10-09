// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.26

using System;
using Xtensive.Orm;

namespace Xtensive.Orm.Security.Tests.Model
{
  [HierarchyRoot]
  public class Branch : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public Branch(Session session)
      : base(session)
    {}
  }
}