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
    private readonly Queue<string> operations = new Queue<string>();

    public readonly int FieldIndex;
    public readonly Expression Source;
    public bool HasOperations { get { return operations.Count > 0; } }

    public void EnqueueOperation(string operation)
    {
      ArgumentValidator.EnsureArgumentNotNull(operation, "operation");
      operations.Enqueue(operation);
    }

    public string DequeueOperation()
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