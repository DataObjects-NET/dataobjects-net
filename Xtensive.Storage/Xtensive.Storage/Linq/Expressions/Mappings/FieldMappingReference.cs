// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.06

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Storage.Linq.Expressions.Mappings
{
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
          complexMapping.Fields.Add(key, segment);
      }
    }

    public void RegisterJoined(string key, ComplexFieldMapping value)
    {
      if (FillMapping) {
        var complexMapping = Mapping as ComplexFieldMapping;
        if (complexMapping != null && !complexMapping.JoinedFields.ContainsKey(key))
          complexMapping.JoinedFields.Add(key, value);
      }
    }

    public void RegisterAnonymous(string key, Expression projection)
    {
      if (FillMapping) {
        var complexMapping = Mapping as ComplexFieldMapping;
        if (complexMapping != null && !complexMapping.AnonymousFields.ContainsKey(key))
          complexMapping.AnonymousFields.Add(key, projection);
      }
    }

    public void RegisterPrimitive(Segment<int> segment)
    {
      if (FillMapping)
        Mapping = new PrimitiveFieldMapping(segment);
    }

    #endregion

    public void Fill(FieldMappingReference fieldMappingRef, MemberType memberType, Expression projection, string memberName, Func<string, string, string> rename)
    {
      if (fillMapping && fieldMappingRef.FillMapping)
        Fill(fieldMappingRef.Mapping, memberType, projection, memberName, rename);
    }

    public void Fill(FieldMapping fieldMapping, MemberType memberType, Expression projection, string memberName, Func<string, string, string> rename)
    {
      if (fillMapping) {
        if (fieldMapping is PrimitiveFieldMapping)
          RegisterFieldMapping(memberName, ((PrimitiveFieldMapping)fieldMapping).Segment);
        else {
          var complexMapping = (ComplexFieldMapping)fieldMapping;
          foreach (var p in complexMapping.Fields)
            RegisterFieldMapping(rename(p.Key, memberName), p.Value);
          foreach (var p in complexMapping.JoinedFields)
            RegisterJoined(rename(p.Key, memberName), p.Value);
          foreach (var p in complexMapping.AnonymousFields)
            RegisterAnonymous(rename(p.Key, memberName), p.Value);
          if (memberType==MemberType.Anonymous || memberType==MemberType.Entity) {
            RegisterJoined(memberName, complexMapping);
            if (memberType==MemberType.Anonymous)
              RegisterAnonymous(memberName, projection);
          }
        }
      }
    }

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