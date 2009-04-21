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
using Xtensive.Storage.Linq.Rewriters;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Linq.Expressions.Mappings
{
  internal sealed class ComplexMapping : IMapping
  {
    internal readonly Dictionary<string, Segment<int>> Fields;
    internal readonly Dictionary<string, ComplexMapping> Entities;
    internal readonly Dictionary<string, Pair<ComplexMapping, Expression>> AnonymousTypes;
    internal readonly Dictionary<string, ComplexMapping> Groupings;
    private readonly List<Pair<string,MemberType>> fillOrder;
    
    #region Accessor methods

    public Segment<int> GetFieldMapping(string fieldName)
    {
      Segment<int> result;
      if (!Fields.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotFindFieldSegmentForFieldX, fieldName));
      return result;
    }

    public bool TryGetJoinedEntity(string fieldName, out ComplexMapping value)
    {
      return Entities.TryGetValue(fieldName, out value);
    }

    public ComplexMapping GetEntityMapping(string fieldName)
    {
      ComplexMapping result;
      if (!Entities.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotFindEntityMappingForFieldX, fieldName));
      return result;
    }

    public ComplexMapping GetGroupingMapping(string fieldName)
    {
      ComplexMapping result;
      if (!Groupings.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotFindEntityMappingForFieldX, fieldName));
      return result;
    }

    public Pair<ComplexMapping, Expression> GetAnonymousMapping(string fieldName)
    {
      Pair<ComplexMapping, Expression> result;
      if (!AnonymousTypes.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotFindAnonymousMappingForFieldX, fieldName));
      return result;
    }

    #endregion

    public List<int> GetColumns(bool entityAsKey)
    {
      var result = new List<int>();
      var lookup = result.ToLookup(i => i);
      if (fillOrder.Count == 0) {
        // mapping for entity
        if (entityAsKey)
          result.AddRange(GetFieldMapping(StorageWellKnown.Key).GetItems());
        else
          result.AddRange(CalculateMemberSegment().GetItems());
      }
      else
        foreach (var pair in fillOrder)
          switch (pair.Second) {
            case MemberType.Primitive:
              result.AddRange(GetFieldMapping(pair.First).GetItems());
              break;
            case MemberType.Entity:
              result.AddRange(GetEntityMapping(pair.First).GetColumns(entityAsKey));
              break;
            case MemberType.Anonymous:
              result.AddRange(GetAnonymousMapping(pair.First).First.GetColumns(entityAsKey));
              break;
            case MemberType.Grouping:
              result.AddRange(GetGroupingMapping(pair.First).GetColumns(entityAsKey));
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
      return result.Distinct().ToList();
    }

    public IMapping CreateShifted(int offset)
    {
      var shiftedFields = Fields.ToDictionary(fm => fm.Key, fm => new Segment<int>(offset + fm.Value.Offset, fm.Value.Length));
      var shiftedEntities = Entities.ToDictionary(jr => jr.Key, jr => (ComplexMapping)jr.Value.CreateShifted(offset));
      var shiftedGroupings = Groupings.ToDictionary(jr => jr.Key, jr => (ComplexMapping)jr.Value.CreateShifted(offset));
      var shiftedAnonymous = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      foreach (var pair in AnonymousTypes) {
        var mapping = pair.Value.First.CreateShifted(offset);
        // TODO: rewrite tuple access
        var expression = pair.Value.Second;
        shiftedAnonymous.Add(pair.Key, new Pair<ComplexMapping, Expression>((ComplexMapping)mapping, expression));
      }
      return new ComplexMapping(shiftedFields, shiftedEntities, shiftedAnonymous, shiftedGroupings, fillOrder);
    }

    public Segment<int> GetMemberSegment(MemberPath fieldPath)
    {
      if (fieldPath.Count == 0) {
        if (fieldPath.PathType == MemberType.Structure || fieldPath.PathType == MemberType.Entity)
          return CalculateMemberSegment();
        throw new InvalidOperationException();
      }
      List<MemberPathItem> pathList = fieldPath.ToList();
      ComplexMapping mapping = this;
      for (int i = 0; i < pathList.Count - 1; i++) {
        MemberPathItem pathItem = pathList[i];
        if (pathItem.Type == MemberType.Entity)
          mapping = mapping.GetEntityMapping(pathItem.Name);
        else if (pathItem.Type == MemberType.Anonymous)
          mapping = mapping.GetAnonymousMapping(pathItem.Name).First;
        else if (pathItem.Type == MemberType.Grouping)
          mapping = mapping.GetGroupingMapping(pathItem.Name);
      }
      MemberPathItem lastItem = pathList.Last();
      if (lastItem.Type == MemberType.Anonymous || lastItem.Type == MemberType.Grouping)
        throw new InvalidOperationException();

      if (lastItem.Type == MemberType.Entity) {
        mapping = mapping.GetEntityMapping(lastItem.Name);
        return mapping.CalculateMemberSegment();
      }
      return mapping.GetFieldMapping(lastItem.Name);
    }

    public IMapping GetMemberMapping(MemberPath fieldPath)
    {
      List<MemberPathItem> pathList = fieldPath.ToList();
      if (pathList.Count == 0)
        return this;
      ComplexMapping mapping = this;
      foreach (MemberPathItem pathItem in pathList) {
        if (pathItem.Type == MemberType.Entity)
          mapping = mapping.GetEntityMapping(pathItem.Name);
        else if (pathItem.Type == MemberType.Anonymous)
          mapping = mapping.GetAnonymousMapping(pathItem.Name).First;
        else if (pathItem.Type == MemberType.Grouping)
          mapping = mapping.GetGroupingMapping(pathItem.Name);
      }
      return mapping;
    }

    public IMapping RewriteColumnIndexes(ItemProjectorRewriter rewriter)
    {
      var columnMapping = rewriter.Mappings;
      var rewritedFields = Fields.ToDictionary(fm => fm.Key, fm => new Segment<int>(columnMapping.IndexOf(fm.Value.Offset), fm.Value.Length));
      var rewritedEntities = Entities.ToDictionary(jr => jr.Key, jr => (ComplexMapping)jr.Value.RewriteColumnIndexes(rewriter));
      var rewritedGroupings = Groupings.ToDictionary(jr => jr.Key, jr => (ComplexMapping)jr.Value.RewriteColumnIndexes(rewriter));
      var rewritedAnonymous = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      foreach (var pair in AnonymousTypes) {
        var mapping = pair.Value.First.RewriteColumnIndexes(rewriter);
        var expression = rewriter.Rewrite(pair.Value.Second);
        rewritedAnonymous.Add(pair.Key, new Pair<ComplexMapping, Expression>((ComplexMapping)mapping, expression));
      }
      return new ComplexMapping(rewritedFields, rewritedEntities, rewritedAnonymous, rewritedGroupings, fillOrder);
    }

    public void Fill(IMapping mapping)
    {
      if (mapping is PrimitiveMapping) {
        var pfm = (PrimitiveMapping)mapping;
        RegisterField(string.Empty, pfm.Segment);
      }
      else {
        var cfm = (ComplexMapping)mapping;
        foreach (var pair in cfm.Fields)
          RegisterField(pair.Key, pair.Value);
        foreach (var pair in cfm.Entities)
          RegisterEntity(pair.Key, pair.Value);
        foreach (var pair in cfm.Groupings)
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
        fillOrder.Add(new Pair<string, MemberType>(key, MemberType.Primitive));
      }
    }
    public void RegisterJoinedEntity(string key, ComplexMapping value)
    {
      Entities.Add(key, value);
    }

    public void RegisterEntity(string key, ComplexMapping value)
    {
      if (!Entities.ContainsKey(key)) {
        Entities.Add(key, value);
        fillOrder.Add(new Pair<string, MemberType>(key, MemberType.Entity));
      }
    }

    public void RegisterGrouping(string key, ComplexMapping value)
    {
      if (!Groupings.ContainsKey(key)) {
        Groupings.Add(key, value);
        fillOrder.Add(new Pair<string, MemberType>(key, MemberType.Grouping));
      }
    }

    public void RegisterAnonymous(string key, ComplexMapping anonymousMapping, Expression projection)
    {
      if (!AnonymousTypes.ContainsKey(key)) {
        AnonymousTypes.Add(key, new Pair<ComplexMapping, Expression>(anonymousMapping, projection));
        fillOrder.Add(new Pair<string, MemberType>(key, MemberType.Anonymous));
      }
    }

    #endregion


    // Constructors

    public ComplexMapping()
    {
      Fields = new Dictionary<string, Segment<int>>();
      Entities = new Dictionary<string, ComplexMapping>();
      AnonymousTypes = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      Groupings = new Dictionary<string, ComplexMapping>();
      fillOrder = new List<Pair<string, MemberType>>();
    }

    public ComplexMapping(TypeInfo type, int offset)
    {
      var fields = new Dictionary<string, Segment<int>>();
      var keySegment = new Segment<int>(offset, type.Hierarchy.KeyInfo.Fields.Sum(pair => pair.Key.MappingInfo.Length));
      fields.Add(StorageWellKnown.Key, keySegment);
      foreach (var field in type.Fields) {
        fields.Add(field.Name, new Segment<int>(offset + field.MappingInfo.Offset, field.MappingInfo.Length));
        if (field.IsEntity)
          fields.Add(field.Name + ".Key", new Segment<int>(offset + field.MappingInfo.Offset, field.MappingInfo.Length));
      }

      Fields = fields;
      Entities = new Dictionary<string, ComplexMapping> ();
      AnonymousTypes = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      Groupings = new Dictionary<string, ComplexMapping>();
      fillOrder = new List<Pair<string, MemberType>>();
    }

    public ComplexMapping(Dictionary<string, Segment<int>> fields)
    {
      Fields = fields;
      Entities = new Dictionary<string, ComplexMapping>();
      AnonymousTypes = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      Groupings = new Dictionary<string, ComplexMapping>();
      fillOrder = new List<Pair<string, MemberType>>();
    }

    private ComplexMapping(
      Dictionary<string, Segment<int>> fields, 
      Dictionary<string, ComplexMapping> joinedFields, 
      Dictionary<string, Pair<ComplexMapping, Expression>> anonymousFields,
      Dictionary<string, ComplexMapping> groupings,
      List<Pair<string, MemberType>> fillOrder)
    {
      Fields = fields;
      Entities = joinedFields;
      AnonymousTypes = anonymousFields;
      Groupings = groupings;
      this.fillOrder = fillOrder;
    }
  }
}