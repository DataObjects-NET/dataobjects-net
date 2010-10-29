// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.27

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel.Version1
{
  [HierarchyRoot]
  public class Category : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }
    [Field]
    public Author Author { get; set; }
    [Field]
    public Category Category { get; set; }
    [Field(Length = 20000)]
    public string LongText { get; set; }
  }
}