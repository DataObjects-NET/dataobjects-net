// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.22

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// Base class for <see cref="ILockable"/> implementors.
  /// </summary>
  [Serializable]
  public abstract class LockableBase: ILockable
  {
    private bool isLocked;

    /// <inheritdoc/>
    [DebuggerHidden]
    public bool IsLocked
    {
      get { return isLocked; }
    }

    /// <inheritdoc/>
    public void Lock()
    {
      Lock(true);
    }

    /// <inheritdoc/>
    public virtual void Lock(bool recursive)
    {
      isLocked = true;
    }

    /// <summary>
    /// Unlocks the object.
    /// Sets <see cref="IsLocked"/> to <see langword="false"/>.
    /// </summary>
    protected void Unlock()
    {
      isLocked = false;
    }
    

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected LockableBase()
      : this (false)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="isLocked">Initial <see cref="IsLocked"/> property value.</param>
    protected LockableBase(bool isLocked)
    {
      this.isLocked = isLocked;
    }
  }
}