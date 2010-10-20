// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Core
{
  /// <summary>
  /// Lockable contract.
  /// Should be implemented by classes, that can
  /// became immutable at some point of time.
  /// </summary>
  public interface ILockable
  {
    /// <summary>
    /// Determines whether the instance of class implementing this interface
    /// is immutable (locked).
    /// </summary>
    /// <remarks>
    /// The implementor of setter of this property should consider, that this
    /// property should not change its value from <see langword="true"/> to 
    /// <see langword="false"/>.
    /// </remarks>
    bool IsLocked { get; }

    /// <summary>
    /// Locks the instance (non-recursively).
    /// </summary>
    void Lock();

    /// <summary>
    /// Locks the instance and (possibly) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    void Lock(bool recursive);
  }
}
