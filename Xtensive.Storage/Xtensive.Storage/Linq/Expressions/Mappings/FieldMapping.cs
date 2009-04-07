// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.24

using System.Collections.Generic;

namespace Xtensive.Storage.Linq.Expressions.Mappings
{
  internal abstract class FieldMapping
  {
    public abstract FieldMapping ShiftOffset(int offset);
    public abstract IList<int> GetColumns();
    public abstract FieldMapping Clone();
  }
}