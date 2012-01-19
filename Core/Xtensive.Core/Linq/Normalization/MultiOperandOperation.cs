// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Linq.Normalization
{
  /// <summary>
  /// An abstract base class for any operation with multiple operands.
  /// </summary>
  /// <typeparam name="T">The type of operand.</typeparam>
  [Serializable]
  public abstract class MultiOperandOperation<T> : IExpressionSource
  {
    /// <summary>
    /// Gets the operands.
    /// </summary>
    public HashSet<T> Operands { get; private set; }

    /// <summary>
    /// Creates equivalent <see cref="Expression"/> object.
    /// </summary>
    public abstract Expression ToExpression();


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected MultiOperandOperation()
    {
      Operands = new HashSet<T>();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="single">The single operand.</param>
    protected MultiOperandOperation(T single)
    {
      Operands = new HashSet<T>() { single };
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="operands">The initial <see cref="Operands"/> content.</param>
    protected MultiOperandOperation(IEnumerable<T> operands)
    {
      Operands = new HashSet<T>(operands);
    }
  }
}