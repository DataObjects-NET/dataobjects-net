// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.29

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Default <see cref="IOperationDescriptor"/> factory - i.e. a factory
  /// used by <see cref="AtomicityContextBase"/>.
  /// </summary>
  public class OperationDescriptorFactory: IOperationDescriptorFactory
  {
    public IRedoDescriptor CreateRedoDescriptor()
    {
      return new RedoDescriptor();
    }

    public IUndoDescriptor CreateUndoDescriptor()
    {
      return new UndoDescriptor();
    }

    public IUndoDescriptor CreateGroupUndoDescriptor()
    {
      return new GroupUndoDescriptor();
    }
  }
}