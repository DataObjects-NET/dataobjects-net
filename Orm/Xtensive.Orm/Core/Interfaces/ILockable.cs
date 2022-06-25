// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
    /// Locks the instance recursively.
    /// </summary>
    void Lock();

    /// <summary>
    /// Locks the instance and (possibly) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    void Lock(bool recursive);
  }
}
