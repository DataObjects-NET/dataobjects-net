// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Linq.Normalization
{
  /// <summary>
  /// An operation with many operands.
  /// </summary>
  /// <typeparam name="T">The type of operands.</typeparam>
  [Serializable]
  public abstract class MultioperandOperation<T>
  {
    /// <summary>
    /// Gets the operands.
    /// </summary>
    public HashSet<T> Operands { get; private set; }

    /// <summary>
    /// Returns equivalent <see cref="Expression"/>.
    /// </summary>
    public abstract Expression ToExpression();


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected MultioperandOperation()
    {
      Operands = new HashSet<T>();
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="operands">Operands.</param>
    /// <param name="operandSets">The other operand sets.</param>
    protected MultioperandOperation(IEnumerable<T> operands, params IEnumerable<T>[] operandSets)
    {
      Operands = new HashSet<T>(operands);

      foreach (var set in operandSets) {
        foreach (var operand in set) {
          Operands.Add(operand);   
        }
      }
    }

    protected MultioperandOperation(T operand, params T[] operands)
    {
      Operands = new HashSet<T>(operands) {operand};
    }
  }
}