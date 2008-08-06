// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Default implementation for <see cref="IUndoDescriptor"/>.
  /// Undo operation descriptor.
  /// </summary>
  [Serializable]
  public class UndoDescriptor: OperationDescriptor,
    IUndoDescriptor
  {
    private IRedoDescriptor oppositeDescriptor;
    private IGroupUndoDescriptor group;
    private IDictionary<string, object> arguments = new Dictionary<string, object>();
    private bool isCompleted;

    [DebuggerHidden]
    public IRedoDescriptor OppositeDescriptor
    {
      get { return oppositeDescriptor; }
      set { oppositeDescriptor = value; }
    }

    [DebuggerHidden]
    public virtual IGroupUndoDescriptor Group
    {
      get { return group; }
      set { group = value; }
    }

    [DebuggerHidden]
    public virtual IDictionary<string, object> Arguments
    {
      get { return arguments; }
    }

    [DebuggerHidden]
    public bool IsCompleted
    {
      get { return isCompleted; }
    }

    public void Complete()
    {
      if (isCompleted)
        throw new InvalidOperationException(Strings.ExAlreadyCompleted);
      isCompleted = true;
    }

    public override void Invoke()
    {
      if (!isCompleted)
        throw Exceptions.InternalError("UndoDescriptor.IsCompleted==false.", Log.Instance);
      CallDescriptor.Method.Invoke(CallDescriptor.Target, new object[] {this});
    }

    public override void Finalize(bool isUndone)
    {
      if (isUndone) {
        if (group!=null) {
          IList<IUndoDescriptor> operations = group.Operations;
          int lastOperationIndex = operations.Count - 1;
          if (lastOperationIndex<0)
            throw Exceptions.InternalError("Wrong UndoDescriptor.Operations structure.", Log.Instance);
          if (operations[lastOperationIndex]!=this)
            throw Exceptions.InternalError("Wrong UndoDescriptor.Operations structure.", Log.Instance);
          operations.RemoveAt(lastOperationIndex);
          group = null;
        }
      }
    }

    public override string ToString()
    {
      if (OppositeDescriptor==null)
        return String.Format("UndoDescriptor: {0}, {1} arguments", base.ToString(), arguments.Count);
      else
        return String.Format("UndoDescriptor: {0}, {1} arguments, for {2}", base.ToString(), arguments.Count, oppositeDescriptor);
    }

    #region IContext<UndoScope> Members

    public bool IsActive {
      get {
        return UndoScope.CurrentDescriptor==this;
      }
    }

    IDisposable IContext.Activate()
    {
      return Activate();
    }

    public UndoScope Activate()
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}