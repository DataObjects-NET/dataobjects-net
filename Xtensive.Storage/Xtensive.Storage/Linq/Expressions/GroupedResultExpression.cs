// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.06

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Mappings;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Expressions
{
  internal sealed class GroupedResultExpression : ResultExpression
  {
    public ResultExpression Value { get; private set; }

    // Constructors

    public GroupedResultExpression(
      Type type, 
      RecordSet recordSet, 
      FieldMapping mapping, 
      Expression<Func<RecordSet, object>> projector, 
      LambdaExpression itemProjector, 
      ResultExpression value)
      : base(type, recordSet, mapping, projector, itemProjector)
    {
      Value = value;
    }
  }
}