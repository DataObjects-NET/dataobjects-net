// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

using System;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// <see cref="ILockable"/> related extension methods.
  /// </summary>
  public static class LockableExtensions
  {
    /// <summary>
    /// Locks the instance (non-recursively).
    /// </summary>
    /// <param name="lockable">Lockable object to lock. Can be <see langword="null"/>.</param>
    public static void LockSafely(this ILockable lockable)
    {
      if (lockable!=null)
        lockable.Lock();
    }

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="lockable">Lockable object to lock. Can be <see langword="null"/>.</param>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    public static void LockSafely(this ILockable lockable, bool recursive)
    {
      if (lockable!=null)
        lockable.Lock(recursive);
    }

    /// <summary>
    /// Ensures <paramref name="lockable"/> is not locked (see <see cref="ILockable.Lock()"/>) yet.
    /// </summary>
    /// <param name="lockable">Lockable object to check.</param>
    /// <exception cref="InstanceIsLockedException">Specified instance is locked.</exception>
    public static void EnsureNotLocked(this ILockable lockable)
    {
      ArgumentValidator.EnsureArgumentNotNull(lockable, "lockable");
      if (lockable.IsLocked)
        throw new InstanceIsLockedException(Strings.ExInstanceIsLocked);
    }

    /// <summary>
    /// Clones <paramref name="lockable"/> if it is not locked
    /// otherwise returns <paramref name="lockable"/> itself.
    /// </summary>
    /// <param name="lockable"><see cref="ILockable"/> to clone.</param>
    /// <returns><paramref name="lockable"/> if it is locked;
    /// clone of <paramref name="lockable"/> if it is not locked and
    /// is <see cref="ICloneable"/>;
    /// <see langword="null"/> otherwise.
    /// </returns>
    public static ILockable CloneIfNotLocked(this ILockable lockable)
    {
      if (lockable.IsLocked)
        return lockable;

      var cloneable = lockable as ICloneable;
      if (cloneable == null)
        return null;
      
      return (ILockable)cloneable.Clone();
    }
  }
}