// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.24

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Storage.Linq.Expressions.Mappings
{
  internal abstract class Mapping
  {
    public abstract Mapping CreateShifted(int offset);
    public abstract List<int> GetColumns(bool entityAsKey);
    public abstract Segment<int> GetMemberSegment(MemberPath fieldPath);
    public abstract Mapping GetMemberMapping(MemberPath fieldPath);
  }
}