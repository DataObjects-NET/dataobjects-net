// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  public sealed class ResultExpression : Expression
  {
    private readonly Dictionary<string,int> fieldMapping = new Dictionary<string, int>();

    public RecordSet RecordSet { get; private set; }
    // TODO: => IsSingleResult
    public bool IsMultipleResults { get; private set; }
    public Func<RecordSet, object> Shaper { get; private set; }
    
    public int GetColumnIndex(string fieldName)
    {
      int value;
      if (fieldMapping.TryGetValue(fieldName, out value))
        return value;
      return -1;
    }


    // Constructors

    public ResultExpression(Type type, RecordSet recordSet, Func<RecordSet,object> shaper, bool isMultiple)
      : base(ExpressionType.Constant, type)
    {
      RecordSet = recordSet;
      Shaper = shaper;
      IsMultipleResults = isMultiple;
    }
  }
}