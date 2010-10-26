// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.07.03

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal class FirstSingleInfo
  {
    public int[] Columns { get; private set; }
    public EntityInfo[] Entities { get; private set; }


    // Constructors

    public FirstSingleInfo(int[] columns, EntityInfo[] entities)
    {
      Columns = columns;
      Entities = entities;
    }
  }
}