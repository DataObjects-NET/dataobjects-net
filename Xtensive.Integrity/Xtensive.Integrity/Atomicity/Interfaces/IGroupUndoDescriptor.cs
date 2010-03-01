// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.28

using System.Collections.Generic;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Describes group undo operation - an operation involving
  /// undoing a set of nested undo operations.
  /// </summary>
  public interface IGroupUndoDescriptor: IUndoDescriptor
  {
    // TODO: Refactor to IDeque<IUndoDescriptor>
    IList<IUndoDescriptor> Operations { get; }
  }
}