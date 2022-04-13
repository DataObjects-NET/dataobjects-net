// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2007.11.22

using System;
using System.Diagnostics;


namespace Xtensive.Core
{
  /// <summary>
  /// Base class for <see cref="ILockable"/> implementors.
  /// </summary>
  [Serializable]
  public abstract class LockableBase: ILockable
  {
    /// <inheritdoc/>
    public bool IsLocked { [DebuggerStepThrough] get; private set; }

    /// <inheritdoc/>
    public void Lock() => Lock(true);

    /// <inheritdoc/>
    public virtual void Lock(bool recursive) =>
      IsLocked = true;

    // Copy of LockableExtensions.EnsureNotLocked() for efficiency (no null checking)
    protected void EnsureNotLocked()
    {
      if (IsLocked) {
        throw new InstanceIsLockedException(Strings.ExInstanceIsLocked);
      }
    }

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="isLocked">Initial <see cref="IsLocked"/> property value.</param>
    protected LockableBase(bool isLocked = false) =>
      IsLocked = isLocked;
  }
}
