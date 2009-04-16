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
  internal class MappingReference
  {
    private readonly bool fillMapping;
    private Mapping mapping;

    // TODO: remove
    public bool FillMapping
    {
      get { return fillMapping; }
    }

    // TODO: -> Value
    public Mapping Mapping
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

    public void RegisterField(string key, Segment<int> segment)
    {
      if (FillMapping) {
        var complexMapping = Mapping as ComplexMapping;
        if (complexMapping != null && !complexMapping.Fields.ContainsKey(key))
          complexMapping.RegisterField(key, segment);
      }
    }

    public void RegisterEntity(string key, ComplexMapping value)
    {
      if (FillMapping) {
        var complexMapping = Mapping as ComplexMapping;
        if (complexMapping != null && !complexMapping.Entities.ContainsKey(key))
          complexMapping.RegisterEntity(key, value);
      }
    }

    public void RegisterAnonymous(string key, ComplexMapping anonymousMapping, Expression projection)
    {
      if (FillMapping) {
        var complexMapping = Mapping as ComplexMapping;
        if (complexMapping != null && !complexMapping.AnonymousTypes.ContainsKey(key)) {
          complexMapping.RegisterAnonymous(key, anonymousMapping, projection);
        }
      }
    }

    public void RegisterPrimitive(Segment<int> segment)
    {
      if (FillMapping)
        Mapping = new PrimitiveMapping(segment);
    }

    #endregion

    public void Replace(Mapping mapping)
    {
      if (fillMapping)
        this.mapping = mapping;
    }


    // Constructors

    public MappingReference()
      : this(true)
    {}

    public MappingReference(bool fillMapping)
    {
      this.fillMapping = fillMapping;
      if (this.fillMapping)
        Mapping = new ComplexMapping();
    }

  }
}