// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05

namespace Xtensive.Orm.Tests
{
  public enum TypeIdBehavior
  {
    Default = AsIs,
    AsIs = 0,
    Include = 1,
    Exclude = 2,
  }
}