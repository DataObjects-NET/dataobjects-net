// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.06

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Storage.Linq.Expressions.Mappings
{
  [DebuggerDisplay("Fill: {FillMapping}, Mapping: {Mapping}")]
  internal class FieldMappingReference
  {
    private readonly bool fillMapping;
    private FieldMapping mapping;

    public bool FillMapping
    {
      get { return fillMapping; }
    }

    public FieldMapping Mapping
    {
      get
      {
        if (!fillMapping)
          throw new InvalidOperationException();
        return mapping;
      }
      private set
      {
        if (!fillMapping)
          throw new InvalidOperationException();
        mapping = value;
      }
    }

    #region Register methods

    public void RegisterFieldMapping(string key, Segment<int> segment)
    {
      if (FillMapping) {
        var complexMapping = Mapping as ComplexFieldMapping;
        if (complexMapping != null && !complexMapping.Fields.ContainsKey(key))
          complexMapping.RegisterFieldMapping(key, segment);
      }
    }

    public void RegisterJoined(string key, ComplexFieldMapping value)
    {
      if (FillMapping) {
        var complexMapping = Mapping as ComplexFieldMapping;
        if (complexMapping != null && !complexMapping.JoinedFields.ContainsKey(key))
          complexMapping.RegisterJoin(key, value);
      }
    }

    public void RegisterAnonymous(string key, ComplexFieldMapping anonymousMapping, Expression projection)
    {
      if (FillMapping) {
        var complexMapping = Mapping as ComplexFieldMapping;
        if (complexMapping != null && !complexMapping.AnonymousFields.ContainsKey(key)) {
          complexMapping.RegisterAnonymous(key, anonymousMapping, projection);
        }
      }
    }

    public void RegisterPrimitive(Segment<int> segment)
    {
      if (FillMapping)
        Mapping = new PrimitiveFieldMapping(segment);
    }

    #endregion

    public void Replace(FieldMapping fieldMapping)
    {
      if (fillMapping)
        mapping = fieldMapping;
    }


    // Constructors

    public FieldMappingReference()
      : this(true)
    {}

    public FieldMappingReference(bool fillMapping)
    {
      this.fillMapping = fillMapping;
      if (this.fillMapping)
        Mapping = new ComplexFieldMapping();
    }

  }
}