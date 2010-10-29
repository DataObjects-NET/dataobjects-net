// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.21

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.Upgrade.FullText.Model.Version6
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public string Text { get; set; }
  }
}