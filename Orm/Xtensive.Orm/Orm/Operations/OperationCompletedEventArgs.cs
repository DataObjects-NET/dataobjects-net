// Copyright (C) 2010-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Diagnostics;


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
    /// 	Initializes a new instance of this class.
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