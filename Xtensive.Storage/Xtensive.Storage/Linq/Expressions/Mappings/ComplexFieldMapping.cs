// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.06

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq.Expressions.Mappings
{
//  [DebuggerDisplay("Complex: Fields({Fields.Count}), JoinedFields({JoinedFields.Count}), AnonymousFields({AnonymousFields.Count})")]
  internal sealed class ComplexFieldMapping : FieldMapping
  {
    internal readonly Dictionary<string, Segment<int>> Fields;
    internal readonly Dictionary<string, ComplexFieldMapping> JoinedFields;
    internal readonly Dictionary<string, Pair<ComplexFieldMapping, Expression>> AnonymousFields;

    #region Accessor methods

    public Segment<int> GetFieldSegment(string fieldName)
    {
      Segment<int> result;
      if (!Fields.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format("Could not find field segment for field '{0}'.", fieldName));
      return result;
    }

    public bool TryGetJoined(string fieldName, out ComplexFieldMapping value)
    {
      return JoinedFields.TryGetValue(fieldName, out value);
    }

    public ComplexFieldMapping GetJoinedFieldMapping(string fieldName)
    {
      ComplexFieldMapping result;
      if (!JoinedFields.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format("Could not find joined field mapping for field '{0}'.", fieldName));
      return result;
    }

    public Pair<ComplexFieldMapping, Expression> GetAnonymousMapping(string fieldName)
    {
      Pair<ComplexFieldMapping, Expression> result;
      if (!AnonymousFields.TryGetValue(fieldName, out result))
        throw new InvalidOperationException(string.Format("Could not find anonymous projection for field '{0}'.", fieldName));
      return result;
    }

    #endregion


    public override IList<int> GetColumns()
    {
      var result = new List<int>();
      foreach (var pair in Fields)
        result.AddRange(pair.Value.GetItems());
      return result.Distinct().ToList();
    }

    public override FieldMapping ShiftOffset(int offset)
    {
      var shiftedFields = Fields.ToDictionary(fm => fm.Key, fm => new Segment<int>(offset + fm.Value.Offset, fm.Value.Length));
      var shiftedRelations = JoinedFields.ToDictionary(jr => jr.Key, jr => (ComplexFieldMapping)jr.Value.ShiftOffset(offset));
      return new ComplexFieldMapping(shiftedFields, shiftedRelations);
    }

    #region Register methods

    public void RegisterFieldMapping(string key, Segment<int> value)
    {
      if (!Fields.ContainsKey(key))
        Fields.Add(key, value);
    }

    public void RegisterJoin(string key, ComplexFieldMapping value)
    {
      if (!JoinedFields.ContainsKey(key))
        JoinedFields.Add(key, value);
    }

    public void RegisterAnonymous(string key, ComplexFieldMapping anonymousMapping, Expression projection)
    {
      if (!AnonymousFields.ContainsKey(key))
        AnonymousFields.Add(key, new Pair<ComplexFieldMapping, Expression>(anonymousMapping, projection));
    }

    #endregion

    public override void Fill(FieldMapping fieldMapping)
    {
      if (fieldMapping is PrimitiveFieldMapping) {
        var pfm = (PrimitiveFieldMapping)fieldMapping;
        RegisterFieldMapping(string.Empty, pfm.Segment);
      }
      else {
        var cfm = (ComplexFieldMapping)fieldMapping;
        foreach (var pair in cfm.Fields)
          RegisterFieldMapping(pair.Key, pair.Value);
        foreach (var pair in cfm.JoinedFields)
          RegisterJoin(pair.Key, pair.Value);
        foreach (var pair in cfm.AnonymousFields)
          RegisterAnonymous(pair.Key, pair.Value.First, pair.Value.Second);
      }
    }

    public override string ToString()
    {
      return string.Format("Complex: Fields({0}), JoinedFields({1}), AnonymousFields({2})", 
        Fields.Count, JoinedFields.Count, AnonymousFields.Count);
    }

    
    // Constructors

    public ComplexFieldMapping()
      : this(new Dictionary<string, Segment<int>>())
    {}

    public ComplexFieldMapping(Dictionary<string, Segment<int>> fields)
      : this(fields, new Dictionary<string, ComplexFieldMapping>())
    {}

    public ComplexFieldMapping(Dictionary<string, Segment<int>> fields, Dictionary<string, ComplexFieldMapping> joinedFields)
      : this(fields, joinedFields, new Dictionary<string, Pair<ComplexFieldMapping, Expression>>())
    {}

    private ComplexFieldMapping(Dictionary<string, Segment<int>> fields, Dictionary<string, ComplexFieldMapping> joinedFields, Dictionary<string, Pair<ComplexFieldMapping, Expression>> anonymousFields)
    {
      Fields = fields;
      JoinedFields = joinedFields;
      AnonymousFields = anonymousFields;
    }
  }
}