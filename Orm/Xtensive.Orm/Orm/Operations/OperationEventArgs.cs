// Copyright (C) 2003-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.11.23

using System;
using System.Diagnostics;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Arguments for <see cref="IOperation"/>-related events.
  /// </summary>
  [Serializable]
  public class OperationEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the operation.
    /// </summary>
    public IOperation Operation { get; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="operation">The <see cref="Operation"/> property value.</param>
    public OperationEventArgs(IOperation operation)
    {
      Operation = operation;
    }
  }
}
