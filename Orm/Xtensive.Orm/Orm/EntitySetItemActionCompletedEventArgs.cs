// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.10.23

using System;
using System.Diagnostics;


namespace Xtensive.Orm
{
  /// <summary>
  /// Describes an event related to <see cref="EntitySet{TItem}"/> item action completion.
  /// </summary>
  public class EntitySetItemActionCompletedEventArgs : EntitySetItemEventArgs
  {
    /// <summary>
    /// Gets the exception, if any, that was thrown on setting the field value.
    /// </summary>
    public Exception Exception { get; private set; }


    // Cosntructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entitySet">The entity set.</param>
    /// <param name="item">The item.</param>
    /// <param name="exception">The <see cref="Exception"/> property value.</param>
    public EntitySetItemActionCompletedEventArgs(EntitySetBase entitySet, Entity item, Exception exception)
      : base(entitySet, item)
    {
      Exception = exception;
    }
  }
}