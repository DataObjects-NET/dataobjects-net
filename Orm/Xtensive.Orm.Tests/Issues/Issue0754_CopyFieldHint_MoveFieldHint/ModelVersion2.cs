// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2010.11.17

using System;

namespace Xtensive.Orm.Tests.Issues.Issue0754_CopyFieldHint_MoveFieldHint.ModelVersion2
{
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public X Reference { get; set; }
  }

  public class B : A
  {
  }
}