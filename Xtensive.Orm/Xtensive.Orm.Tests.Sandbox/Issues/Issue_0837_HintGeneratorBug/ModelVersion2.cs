// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using Xtensive.Core;
using Xtensive.Orm.Upgrade;
using System;

namespace Xtensive.Orm.Tests.Issues.Issue_0834_HintGeneratorBug.Model.Version2
{
  [HierarchyRoot]
  public class Base : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string BaseTitle { get;set; }
  }

  [HierarchyRoot]
  public class Derived : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string BaseTitle { get;set; }

    [Field]
    public string DerivedTitle { get;set; }
  }
}