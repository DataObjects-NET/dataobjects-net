// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Storage.Linq
{
  internal class ResultMapping
  {
    public Dictionary<string, Segment<int>> Fields { get; private set; }
    public Dictionary<string, ResultMapping> JoinedRelations { get; private set; }
    public Dictionary<string, Expression> AnonymousProjections { get; private set; }
    public Segment<int> Segment { get; internal set; }

    public ResultMapping ShiftOffset(int offset)
    {
      var shiftedFields = Fields.ToDictionary(fm => fm.Key, fm => new Segment<int>(offset + fm.Value.Offset, fm.Value.Length));
      var shiftedRelations = JoinedRelations.ToDictionary(jr => jr.Key, jr => jr.Value.ShiftOffset(offset));
      return new ResultMapping(shiftedFields, shiftedRelations);
    }

    public void RegisterFieldMapping(string key, Segment<int> segment)
    {
      if (!Fields.ContainsKey(key))
        Fields.Add(key, segment);
    }

    public void RegisterJoined(string key, ResultMapping mapping)
    {
      if (!JoinedRelations.ContainsKey(key))
        JoinedRelations.Add(key, mapping);
    }

    public void RegisterAnonymous(string key, Expression projection)
    {
      if (!AnonymousProjections.ContainsKey(key))
        AnonymousProjections.Add(key, projection);
    }


    // Constructors

    public ResultMapping(Segment<int> segment)
    {
      Fields = new Dictionary<string, Segment<int>>();
      JoinedRelations = new Dictionary<string, ResultMapping>();
      AnonymousProjections = new Dictionary<string, Expression>();
      Segment = segment;
    }

    public ResultMapping()
      : this(new Dictionary<string, Segment<int>>(), new Dictionary<string, ResultMapping>(), new Dictionary<string, Expression>())
    {}

    public ResultMapping(
      Dictionary<string, Segment<int>> fieldMapping, 
      Dictionary<string, ResultMapping> joinedRelations)
      : this(fieldMapping, joinedRelations, new Dictionary<string, Expression>())
    {}

    public ResultMapping(
      Dictionary<string, Segment<int>> fieldMapping, 
      Dictionary<string, ResultMapping> joinedRelations,
      Dictionary<string, Expression> anonymousProjections)
    {
      Fields = fieldMapping;
      JoinedRelations = joinedRelations;
      AnonymousProjections = anonymousProjections;
      if (Fields.Count > 0) {
        var offset = Fields.Min(pair => pair.Value.Offset);
        var endOffset = Fields.Max(pair => pair.Value.Offset);
        var length = endOffset - offset + 1;
        Segment = new Segment<int>(offset, length);
      }
      else
        // TODO: refactor this code to support primitive type projections and empty projections
        Segment = new Segment<int>(0, 1);
//        throw new InvalidOperationException();
    }
  }
}