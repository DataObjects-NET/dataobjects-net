// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.07.03

using System.Collections.Generic;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal class EntityInfo
  {
    public TypeInfo EntityType { get; private set; }
    public int[] KeyColumns { get; private set; }
    public List<int> Columns { get; private set; }


    // Constructors

    public EntityInfo(TypeInfo entityType, int[] keyColumns)
      : this(entityType, keyColumns, new List<int>())
    {}

    public EntityInfo(TypeInfo entityType, int[] keyColumns, List<int> columns)
    {
      EntityType = entityType;
      KeyColumns = keyColumns;
      Columns = columns;
    }
  }
}