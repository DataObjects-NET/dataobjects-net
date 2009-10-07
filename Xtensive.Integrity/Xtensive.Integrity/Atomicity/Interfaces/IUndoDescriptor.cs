// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.28

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.IoC;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Undo operation descriptor.
  /// </summary>
  public interface IUndoDescriptor: IOperationDescriptor, 
    IContext<UndoScope>
  {
    /// <summary>
    /// Gets or sets the opposite descriptor.
    /// </summary>    
    IRedoDescriptor OppositeDescriptor { get; set; }

    /// <summary>
    /// Gets or sets the group undo operation.
    /// </summary>    
    IGroupUndoDescriptor Group { get; set; }

    /// <summary>
    /// Gets the descriptor's arguments.
    /// </summary>s    
    IDictionary<string, object> Arguments { get; }

    /// <summary>
    /// Gets a value indicating whether this descriptor is completed.
    /// </summary>    
    bool IsCompleted { get; }

    /// <summary>
    /// Completes this descriptor.
    /// </summary>
    void Complete();
  }
}