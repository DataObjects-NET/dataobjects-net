// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.20

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Storage.Rse.Optimisation
{
  internal sealed class ExpressionOnTupleField : Expression
  {
    private readonly Queue<OperationInfo> operations = new Queue<OperationInfo>();

    public readonly int FieldIndex;
    public readonly Expression Source;
    public bool HasOperations { get { return operations.Count > 0; } }
    public int OperationsCount { get { return operations.Count; } }

    public void EnqueueOperation(string name, IEnumerable<Expression> arguments, Expression source)
    {
      var info = new OperationInfo(name, arguments, source);
      operations.Enqueue(info);
    }

    public OperationInfo DequeueOperation()
    {
      return operations.Dequeue();
    }

    public ExpressionOnTupleField(int fieldIndex, Expression source)
      :base(source.NodeType, source.Type)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldIndex, 0, int.MaxValue, "fieldIndex");
      FieldIndex = fieldIndex;
      Source = source;
    }
  }
}