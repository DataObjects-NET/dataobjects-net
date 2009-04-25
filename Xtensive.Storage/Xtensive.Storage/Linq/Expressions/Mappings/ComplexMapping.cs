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
    public Dictionary<string, Segment<int>> Fields { get; private set; }
    public Dictionary<string, ComplexMapping> Entities { get; private set; }
    public Dictionary<string, Pair<ComplexMapping, Expression>> AnonymousTypes { get; private set; }
    public Dictionary<string, ComplexMapping> Groupings { get; private set; }
    public Dictionary<string, ComplexMapping> Subqueries { get; private set; }
    public List<Pair<string, MemberType>> FillOrder { get; private set; }

    #region Accessor methods

    public IEnumerable<Pair<string, Segment<int>>> GetFields(IEnumerable<int> columns)
    {
      return Fields.Where(keyValuePair =>
        columns.Any(columnIndex =>
          columnIndex >= keyValuePair.Value.Offset
            && columnIndex < keyValuePair.Value.Offset + keyValuePair.Value.Length))
        .Select(pair => new Pair<string, Segment<int>>(pair.Key, pair.Value));
    }

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
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotFindGroupingMappingForFieldX, fieldName));
      return result;
    }

    public ComplexMapping GetSubqueryMapping(string fieldName)
    {
      ComplexMapping result;
      if (!Subqueries.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotFindSubqueryMappingForFieldX, fieldName));
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
      if (FillOrder.Count==0) {
        // mapping for entity
        if (entityAsKey)
          result.AddRange(GetFieldMapping(StorageWellKnown.Key).GetItems());
        else
          result.AddRange(CalculateMemberSegment().GetItems());
      }
      else
        foreach (var pair in FillOrder)
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
      var shiftedEntities = Entities.ToDictionary(jr => jr.Key, jr => (ComplexMapping) jr.Value.CreateShifted(offset));
      var shiftedGroupings = Groupings.ToDictionary(jr => jr.Key, jr => (ComplexMapping) jr.Value.CreateShifted(offset));
      var shiftedSubqueries = Subqueries.ToDictionary(jr => jr.Key, jr => (ComplexMapping) jr.Value.CreateShifted(offset));
      var shiftedAnonymous = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      foreach (var pair in AnonymousTypes) {
        var mapping = pair.Value.First.CreateShifted(offset);
        // TODO: rewrite tuple access
        var expression = pair.Value.Second;
        shiftedAnonymous.Add(pair.Key, new Pair<ComplexMapping, Expression>((ComplexMapping) mapping, expression));
      }
      return new ComplexMapping(shiftedFields, shiftedEntities, shiftedAnonymous, shiftedGroupings, shiftedSubqueries, FillOrder);
    }

    public Segment<int> GetMemberSegment(MemberPath fieldPath)
    {
      if (fieldPath.Count==0) {
        if (fieldPath.PathType==MemberType.Structure || fieldPath.PathType==MemberType.Entity)
          return CalculateMemberSegment();
        throw new InvalidOperationException();
      }
      List<MemberPathItem> pathList = fieldPath.ToList();
      ComplexMapping mapping = this;
      for (int i = 0; i < pathList.Count - 1; i++) {
        MemberPathItem pathItem = pathList[i];
        if (pathItem.Type==MemberType.Entity)
          mapping = mapping.GetEntityMapping(pathItem.Name);
        else if (pathItem.Type==MemberType.Anonymous)
          mapping = mapping.GetAnonymousMapping(pathItem.Name).First;
        else if (pathItem.Type==MemberType.Grouping)
          mapping = mapping.GetGroupingMapping(pathItem.Name);
      }
      MemberPathItem lastItem = pathList.Last();
      if (lastItem.Type==MemberType.Anonymous || lastItem.Type==MemberType.Grouping)
        throw new InvalidOperationException();

      if (lastItem.Type==MemberType.Entity) {
        mapping = mapping.GetEntityMapping(lastItem.Name);
        return mapping.CalculateMemberSegment();
      }
      return mapping.GetFieldMapping(lastItem.Name);
    }

    public IMapping GetMemberMapping(MemberPath fieldPath)
    {
      List<MemberPathItem> pathList = fieldPath.ToList();
      if (pathList.Count==0)
        return this;
      ComplexMapping mapping = this;
      foreach (MemberPathItem pathItem in pathList) {
        if (pathItem.Type==MemberType.Entity)
          mapping = mapping.GetEntityMapping(pathItem.Name);
        else if (pathItem.Type==MemberType.Anonymous)
          mapping = mapping.GetAnonymousMapping(pathItem.Name).First;
        else if (pathItem.Type==MemberType.Grouping)
          mapping = mapping.GetGroupingMapping(pathItem.Name);
      }
      return mapping;
    }

    public IMapping RewriteColumnIndexes(ItemProjectorRewriter rewriter)
    {
      var columnMapping = rewriter.Mappings;
      var rewrittenFields = Fields.ToDictionary(fm => fm.Key, fm => new Segment<int>(columnMapping.IndexOf(fm.Value.Offset), fm.Value.Length));
      var rewrittenEntities = Entities.ToDictionary(jr => jr.Key, jr => (ComplexMapping) jr.Value.RewriteColumnIndexes(rewriter));
      var rewrittenGroupings = Groupings.ToDictionary(jr => jr.Key, jr => (ComplexMapping) jr.Value.RewriteColumnIndexes(rewriter));
      var rewrittenSubqueries = Subqueries.ToDictionary(jr => jr.Key, jr => (ComplexMapping) jr.Value.RewriteColumnIndexes(rewriter));
      var rewrittenAnonymous = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      foreach (var pair in AnonymousTypes) {
        var mapping = pair.Value.First.RewriteColumnIndexes(rewriter);
        var expression = rewriter.Rewrite(pair.Value.Second);
        rewrittenAnonymous.Add(pair.Key, new Pair<ComplexMapping, Expression>((ComplexMapping) mapping, expression));
      }
      return new ComplexMapping(rewrittenFields, rewrittenEntities, rewrittenAnonymous, rewrittenGroupings, rewrittenSubqueries, FillOrder);
    }

    public void Fill(IMapping mapping)
    {
      if (mapping is PrimitiveMapping) {
        var pfm = (PrimitiveMapping) mapping;
        RegisterField(string.Empty, pfm.Segment);
      }
      else {
        var cfm = (ComplexMapping) mapping;
        foreach (var pair in cfm.Fields)
          RegisterField(pair.Key, pair.Value);
        foreach (var pair in cfm.Entities)
          RegisterEntity(pair.Key, pair.Value);
        foreach (var pair in cfm.Groupings)
          RegisterGrouping(pair.Key, pair.Value);
        foreach (var pair in cfm.Subqueries)
          RegisterSubquery(pair.Key, pair.Value);
        foreach (var pair in cfm.AnonymousTypes)
          RegisterAnonymous(pair.Key, pair.Value.First, pair.Value.Second);
      }
    }

    public override string ToString()
    {
      return string.Format("Complex: Fields({0}), Entities({1}), AnonymousTypes({2})",
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
        FillOrder.Add(new Pair<string, MemberType>(key, MemberType.Primitive));
      }
      else {
        Fields[key] = value;
      }
    }

    public void RegisterJoinedEntity(string key, ComplexMapping value)
    {
      Entities.Add(key, value);
    }

    public void OverwriteJoinedEntity(string key, ComplexMapping value)
    {
      Entities[key] = value;
    }

    public void RegisterEntity(string key, ComplexMapping value)
    {
      if (!Entities.ContainsKey(key)) {
        Entities.Add(key, value);
        FillOrder.Add(new Pair<string, MemberType>(key, MemberType.Entity));
      }
    }

    public void RegisterGrouping(string key, ComplexMapping value)
    {
      if (!Groupings.ContainsKey(key)) {
        Groupings.Add(key, value);
        FillOrder.Add(new Pair<string, MemberType>(key, MemberType.Grouping));
      }
    }

    public void RegisterSubquery(string key, ComplexMapping value)
    {
      if (!Subqueries.ContainsKey(key)) {
        Subqueries.Add(key, value);
        FillOrder.Add(new Pair<string, MemberType>(key, MemberType.Subquery));
      }
    }

    public void RegisterAnonymous(string key, ComplexMapping anonymousMapping, Expression projection)
    {
      if (!AnonymousTypes.ContainsKey(key)) {
        AnonymousTypes.Add(key, new Pair<ComplexMapping, Expression>(anonymousMapping, projection));
        FillOrder.Add(new Pair<string, MemberType>(key, MemberType.Anonymous));
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
      Subqueries = new Dictionary<string, ComplexMapping>();
      FillOrder = new List<Pair<string, MemberType>>();
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
      Entities = new Dictionary<string, ComplexMapping>();
      AnonymousTypes = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      Groupings = new Dictionary<string, ComplexMapping>();
      Subqueries = new Dictionary<string, ComplexMapping>();
      FillOrder = new List<Pair<string, MemberType>>();
    }

    public ComplexMapping(Dictionary<string, Segment<int>> fields)
    {
      Fields = fields;
      Entities = new Dictionary<string, ComplexMapping>();
      AnonymousTypes = new Dictionary<string, Pair<ComplexMapping, Expression>>();
      Groupings = new Dictionary<string, ComplexMapping>();
      Subqueries = new Dictionary<string, ComplexMapping>();
      FillOrder = new List<Pair<string, MemberType>>();
    }

    private ComplexMapping(
      Dictionary<string, Segment<int>> fields,
      Dictionary<string, ComplexMapping> joinedFields,
      Dictionary<string, Pair<ComplexMapping, Expression>> anonymousFields,
      Dictionary<string, ComplexMapping> groupings,
      Dictionary<string, ComplexMapping> subqueries,
      List<Pair<string, MemberType>> fillOrder)
    {
      Fields = fields;
      Entities = joinedFields;
      AnonymousTypes = anonymousFields;
      Groupings = groupings;
      Subqueries = subqueries;
      this.FillOrder = fillOrder;
    }
  }
}