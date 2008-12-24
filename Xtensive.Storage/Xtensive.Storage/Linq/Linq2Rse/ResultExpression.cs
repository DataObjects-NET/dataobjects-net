// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Linq2Rse.Internal;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal sealed class ResultExpression : Expression
  {
    private readonly Dictionary<string,int> fieldMapping = new Dictionary<string, int>();

    public RecordSet RecordSet { get; private set; }
    // TODO: => IsSingleResult
    public bool IsMultipleResults { get; private set; }
    public Func<RecordSet, object> Shaper { get; private set; }
    public TypeMapping[] Mappings { get; private set; }

    
    public int GetColumnIndex(string fieldName)
    {
      int value;
      if (fieldMapping.TryGetValue(fieldName, out value))
        return value;
      return -1;
    }


    // Constructors

    public ResultExpression(Type type, RecordSet recordSet, TypeMapping[] mappings, Func<RecordSet,object> shaper, bool isMultiple)
      : base(ExpressionType.Constant, type)
    {
      RecordSet = recordSet;
      Mappings = mappings;
      Shaper = shaper;
      IsMultipleResults = isMultiple;
      foreach (var column in recordSet.Header.Columns) {
        var calculated = column as CalculatedColumn;
        var aggregate = column as AggregateColumn;
        var mapped = column as MappedColumn;
        if (mapped!=null)
          fieldMapping.Add(mapped.ColumnInfoRef.FieldName, mapped.Index);
      }
    }
  }
}