// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using Xtensive.Orm;

namespace Xtensive.Practices.Security.Tests.Model
{
  [HierarchyRoot]
  public class Customer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public bool IsVip { get; protected set; }

    [Field]
    public bool IsAutomobileIndustry { get; set; }

    [Field]
    public bool IsAircraftIndustry { get; set; }

    public Customer(Session session)
      : base(session)
    {
      IsVip = false;
    }
  }
}