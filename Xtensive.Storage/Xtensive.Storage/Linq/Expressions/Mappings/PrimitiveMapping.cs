// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.06

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using System.Linq;

namespace Xtensive.Storage.Linq.Expressions.Mappings
{
  internal sealed class PrimitiveMapping : Mapping
  {
    private readonly Segment<int> segment;

    public Segment<int> Segment
    {
      get { return segment; }
    }

    // TODO: -> List<int>?
    public override IList<int> GetColumns()
    {
      return segment.GetItems().ToList();
    }

    public override Mapping ShiftOffset(int offset)
    {
      return new PrimitiveMapping(new Segment<int>(segment.Offset + offset, segment.Length));
    }

    public override void Fill(Mapping mapping)
    {
      throw new NotSupportedException();
    }

    public override Segment<int> GetMemberSegment(MemberPath fieldPath)
    {
      if (fieldPath.Count == 0)
        return segment;
      throw new InvalidOperationException();
    }

    public override Mapping GetMemberMapping(MemberPath fieldPath)
    {
      if (fieldPath.Count == 0)
        return this;
      throw new InvalidOperationException();
    }

    public override string ToString()
    {
      return string.Format("Primitive: [{0}, {1}]", segment.Offset, segment.Length);
    }


    // Constructors

    public PrimitiveMapping(Segment<int> segment)
    {
      this.segment = segment;
    }
  }
}