// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.


namespace Xtensive.Core.Threading
{
  /// <summary>
  /// Describes an object that has <see cref="SyncRoot"/> property.
  /// </summary>
  public interface IHasSyncRoot
  {
    /// <summary>
    /// Gets or sets the synchronization root of the object.
    /// </summary>
    object SyncRoot { get; }
  }
}
