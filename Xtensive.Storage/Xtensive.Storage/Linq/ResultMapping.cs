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
    private bool mapsToPrimitive;
    private readonly Dictionary<string, Segment<int>> fields;
    private readonly Dictionary<string, ResultMapping> joinedRelations;
    private Segment<int> segment;

    public bool MapsToPrimitive
    {
      get { return mapsToPrimitive; }
    }

    public Dictionary<string, Segment<int>> Fields
    {
      get { return fields; }
    }

    public Dictionary<string, ResultMapping> JoinedRelations
    {
      get { return joinedRelations; }
    }

    public Dictionary<string, Expression> AnonymousProjections { get; private set; }

    public Segment<int> Segment
    {
      get
      {
        if (!mapsToPrimitive)
          UpdateMappingSegment();
        return segment;
      }
    }

    public ResultMapping ShiftOffset(int offset)
    {
      var shiftedFields = Fields.ToDictionary(fm => fm.Key, fm => new Segment<int>(offset + fm.Value.Offset, fm.Value.Length));
      var shiftedRelations = JoinedRelations.ToDictionary(jr => jr.Key, jr => jr.Value.ShiftOffset(offset));
      return new ResultMapping(shiftedFields, shiftedRelations);
    }

    public void RegisterFieldMapping(string key, Segment<int> segment)
    {
      if (mapsToPrimitive)
        throw new InvalidOperationException();
      if (!Fields.ContainsKey(key))
        Fields.Add(key, segment);
    }

    public void RegisterJoined(string key, ResultMapping mapping)
    {
      if (mapsToPrimitive)
        throw new InvalidOperationException();
      if (!JoinedRelations.ContainsKey(key))
        JoinedRelations.Add(key, mapping);
    }

    public void RegisterAnonymous(string key, Expression projection)
    {
      if (mapsToPrimitive)
        throw new InvalidOperationException();
      if (!AnonymousProjections.ContainsKey(key))
        AnonymousProjections.Add(key, projection);
    }

    public void RegisterPrimitive(Segment<int> primitiveTypeMapping)
    {
      mapsToPrimitive = true;
      segment = primitiveTypeMapping;
    }

    private void UpdateMappingSegment()
    {
      if (Fields.Count > 0) {
        var offset = Fields.Min(pair => pair.Value.Offset);
        var endOffset = Fields.Max(pair => pair.Value.Offset);
        var length = endOffset - offset + 1;
        this.segment = new Segment<int>(offset, length);
      }
      else {
        // TODO: refactor this code to support primitive type projections and empty projections
        this.segment = new Segment<int>(0, 1);
      }
    }


    // Constructors

    public ResultMapping(Segment<int> segment)
    {
      fields = new Dictionary<string, Segment<int>>();
      joinedRelations = new Dictionary<string, ResultMapping>();
      AnonymousProjections = new Dictionary<string, Expression>();
      mapsToPrimitive = true;
      this.segment = segment;
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
      fields = fieldMapping;
      this.joinedRelations = joinedRelations;
      AnonymousProjections = anonymousProjections;
      UpdateMappingSegment();
    }
  }
}