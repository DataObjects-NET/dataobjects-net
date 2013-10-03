// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="ILockable"/> related extension methods.
  /// </summary>
  public static class LockableExtensions
  {
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
  }
}