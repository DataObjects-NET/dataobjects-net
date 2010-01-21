// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.21

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Tests.Upgrade.FullText.Model.Version2
{
  [HierarchyRoot]
  public class Article : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [FullText]
    [Field]
    public string Title { get; private set; }
  }
}