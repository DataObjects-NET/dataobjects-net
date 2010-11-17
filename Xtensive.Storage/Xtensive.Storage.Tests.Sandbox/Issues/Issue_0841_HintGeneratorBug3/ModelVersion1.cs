// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;

namespace Xtensive.Storage.Tests.Issues.Issue_0841_HintGeneratorBug3.Model.Version1
{
  [HierarchyRoot]
  public class Base : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
  }

  public class Derived : Base
  {
    [Field]
    public string Text { get; set; }
  }
}