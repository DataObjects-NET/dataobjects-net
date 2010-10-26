// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Arguments for <see cref="IOperation"/> completion events.
  /// </summary>
  [Serializable]
  public sealed class OperationCompletedEventArgs : OperationEventArgs
  {
    /// <summary>
    /// Gets the completion flag of the operation.
    /// <see langword="True" />, if operation was completed successfully;
    /// otherwise, <see langword="false" />.
    /// </summary>
    public bool IsCompleted { get; private set; }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="operation">The <see cref="Operation"/> property value.</param>
    /// <param name="isCompleted"><see cref="IsCompleted"/> property value.</param>
    public OperationCompletedEventArgs(IOperation operation, bool isCompleted)
      : base(operation)
    {
      IsCompleted = isCompleted;
    }
  }
}