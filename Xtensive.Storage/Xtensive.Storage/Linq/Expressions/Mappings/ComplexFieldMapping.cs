// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Expressions.Mappings
{
  internal sealed class ComplexFieldMapping : FieldMapping
  {
    internal readonly Dictionary<string, Pair<ComplexFieldMapping, Expression>> AnonymousTypes;
    internal readonly Dictionary<string, Segment<int>> Fields;
    internal readonly Dictionary<string, ComplexFieldMapping> Entities;
    private  readonly List<int> columns = new List<int>();

    #region Accessor methods

    public Segment<int> GetFieldMapping(string fieldName)
    {
      Segment<int> result;
      if (!Fields.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format("Could not find field segment for field '{0}'.", fieldName));
      return result;
    }

    public bool TryGetJoinedEntity(string fieldName, out ComplexFieldMapping value)
    {
      return Entities.TryGetValue(fieldName, out value);
    }

    public ComplexFieldMapping GetEntityMapping(string fieldName)
    {
      ComplexFieldMapping result;
      if (!Entities.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format("Could not find joined field mapping for field '{0}'.", fieldName));
      return result;
    }

    public Pair<ComplexFieldMapping, Expression> GetAnonymousMapping(string fieldName)
    {
      Pair<ComplexFieldMapping, Expression> result;
      if (!AnonymousTypes.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format("Could not find anonymous projection for field '{0}'.", fieldName));
      return result;
    }

    #endregion

    public override IList<int> GetColumns()
    {
      return columns.Distinct().ToList();
    }

    public override FieldMapping ShiftOffset(int offset)
    {
      var shiftedFields = Fields.ToDictionary(fm => fm.Key, fm => new Segment<int>(offset + fm.Value.Offset, fm.Value.Length));
      var shiftedRelations = Entities.ToDictionary(jr => jr.Key, jr => (ComplexFieldMapping)jr.Value.ShiftOffset(offset));
      var shiftedAnonymous = new Dictionary<string, Pair<ComplexFieldMapping, Expression>>();
      foreach (var pair in AnonymousTypes) {
        var mapping = pair.Value.First.ShiftOffset(offset);
        // TODO: rewrite tuple access
        var expression = pair.Value.Second;
        shiftedAnonymous.Add(pair.Key, new Pair<ComplexFieldMapping, Expression>((ComplexFieldMapping)mapping, expression));
      }
      return new ComplexFieldMapping(shiftedFields, shiftedRelations, shiftedAnonymous);
    }

    public override Segment<int> GetMemberSegment(MemberPath fieldPath)
    {
      if (fieldPath.Count == 0) {
        if (fieldPath.PathType == MemberType.Structure || fieldPath.PathType == MemberType.Entity)
          return CalculateMemberSegment();
        throw new InvalidOperationException();
      }
      List<MemberPathItem> pathList = fieldPath.ToList();
      ComplexFieldMapping mapping = this;
      for (int i = 0; i < pathList.Count - 1; i++) {
        MemberPathItem pathItem = pathList[i];
        if (pathItem.Type == MemberType.Entity)
          mapping = mapping.GetEntityMapping(pathItem.Name);
        else if (pathItem.Type == MemberType.Anonymous)
          mapping = mapping.GetAnonymousMapping(pathItem.Name).First;
      }
      MemberPathItem lastItem = pathList.Last();
      if (lastItem.Type == MemberType.Anonymous)
        throw new InvalidOperationException();

      if (lastItem.Type == MemberType.Entity) {
        mapping = mapping.GetEntityMapping(lastItem.Name);
        return mapping.CalculateMemberSegment();
      }
      return mapping.GetFieldMapping(lastItem.Name);
    }

    public override FieldMapping GetMemberMapping(MemberPath fieldPath)
    {
      List<MemberPathItem> pathList = fieldPath.ToList();
      if (pathList.Count == 0)
        return this;
      ComplexFieldMapping mapping = this;
      foreach (MemberPathItem pathItem in pathList) {
        if (pathItem.Type == MemberType.Entity)
          mapping = mapping.GetEntityMapping(pathItem.Name);
        else if (pathItem.Type == MemberType.Anonymous)
          mapping = mapping.GetAnonymousMapping(pathItem.Name).First;
      }
      return mapping;
    }

    public override void Fill(FieldMapping fieldMapping)
    {
      if (fieldMapping is PrimitiveFieldMapping) {
        var pfm = (PrimitiveFieldMapping)fieldMapping;
        RegisterField(string.Empty, pfm.Segment);
      }
      else {
        var cfm = (ComplexFieldMapping)fieldMapping;
        foreach (var pair in cfm.Fields)
          RegisterField(pair.Key, pair.Value);
        foreach (var pair in cfm.Entities)
          RegisterEntity(pair.Key, pair.Value);
        foreach (var pair in cfm.AnonymousTypes)
          RegisterAnonymous(pair.Key, pair.Value.First, pair.Value.Second);
      }
    }

    public override string ToString()
    {
      return string.Format("Complex: Fields({0}), JoinedFields({1}), AnonymousFields({2})",
        Fields.Count, Entities.Count, AnonymousTypes.Count);
    }

    private Segment<int> CalculateMemberSegment()
    {
      int offset = Fields.Min(pair => pair.Value.Offset);
      int endOffset = Fields.Max(pair => pair.Value.Offset);
      int length = endOffset - offset + 1;
      return new Segment<int>(offset, length);
    }

    #region Register methods

    public void RegisterField(string key, Segment<int> value)
    {
      if (!Fields.ContainsKey(key)) {
        Fields.Add(key, value);
        columns.AddRange(value.GetItems());
      }
    }

    public void RegisterEntity(string key, ComplexFieldMapping value)
    {
      if (!Entities.ContainsKey(key))
        Entities.Add(key, value);
    }

    public void RegisterAnonymous(string key, ComplexFieldMapping anonymousMapping, Expression projection)
    {
      if (!AnonymousTypes.ContainsKey(key))
        AnonymousTypes.Add(key, new Pair<ComplexFieldMapping, Expression>(anonymousMapping, projection));
    }

    #endregion


    // Constructors

    public ComplexFieldMapping()
      : this(new Dictionary<string, Segment<int>>(), new Dictionary<string, ComplexFieldMapping>(), new Dictionary<string, Pair<ComplexFieldMapping, Expression>>())
    {}

    public ComplexFieldMapping(TypeInfo type, int offset)
    {
      var fields = new Dictionary<string, Segment<int>>();
      var keySegment = new Segment<int>(offset, type.Hierarchy.KeyInfo.Fields.Sum(pair => pair.Key.MappingInfo.Length));
      fields.Add("Key", keySegment);
      foreach (var field in type.Fields) {
        fields.Add(field.Name, new Segment<int>(offset + field.MappingInfo.Offset, field.MappingInfo.Length));
        if (field.IsEntity)
          fields.Add(field.Name + ".Key", new Segment<int>(offset + field.MappingInfo.Offset, field.MappingInfo.Length));
      }

      Fields = fields;
      Entities = new Dictionary<string, ComplexFieldMapping> ();
      AnonymousTypes = new Dictionary<string, Pair<ComplexFieldMapping, Expression>>();

      columns.AddRange(Enumerable.Range(offset, type.Columns.Count));
    }

    private ComplexFieldMapping(Dictionary<string, Segment<int>> fields)
      : this (fields, new Dictionary<string, ComplexFieldMapping>(), new Dictionary<string, Pair<ComplexFieldMapping, Expression>>())
    {}

    private ComplexFieldMapping(Dictionary<string, Segment<int>> fields, Dictionary<string, ComplexFieldMapping> joinedFields, Dictionary<string, Pair<ComplexFieldMapping, Expression>> anonymousFields)
    {
      Fields = fields;
      Entities = joinedFields;
      AnonymousTypes = anonymousFields;
    }
  }
}