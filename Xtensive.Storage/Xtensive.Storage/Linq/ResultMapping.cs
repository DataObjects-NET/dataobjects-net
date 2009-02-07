// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.24

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Storage.Linq
{
  internal class ResultMapping
  {
    public Dictionary<string, Segment<int>> Fields { get; private set; }
    public Dictionary<string, ResultMapping> JoinedRelations { get; private set; }
    public Segment<int> Segment { private set; get; }

    public ResultMapping ShiftOffset(int offset)
    {
      var shiftedFields = Fields.ToDictionary(fm => fm.Key, fm => new Segment<int>(offset + fm.Value.Offset, fm.Value.Length));
      var shiftedRelations = JoinedRelations.ToDictionary(jr => jr.Key, jr => jr.Value.ShiftOffset(offset));
      return new ResultMapping(shiftedFields, shiftedRelations);
    }


    // Constructors

    public ResultMapping(Dictionary<string, Segment<int>> fieldMapping, Dictionary<string, ResultMapping> joinedRelations)
    {
      Fields = fieldMapping;
      JoinedRelations = joinedRelations;
      if (Fields.Count > 0)
        Segment = new Segment<int>(Fields.Min(pair => pair.Value.Offset), Fields.Max(pair => pair.Value.Offset) + 1);
      else
        // TODO: refecator this code to support primitive type projections and empty projections
        Segment = new Segment<int>(0,1);
    }
  }
}