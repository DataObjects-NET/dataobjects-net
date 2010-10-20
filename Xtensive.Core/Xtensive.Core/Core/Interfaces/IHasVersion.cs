// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Core
{
  /// <summary>
  /// Allows to track changes in object by its <see cref="Version"/>.
  /// </summary>
  public interface IHasVersion
  {
    /// <summary>
    /// Gets object version.
    /// Object isn't changed, while its
    /// <c>oldVersion.Equals(newVersion)</c>.
    /// </summary>
    object Version { get; }
  }

  /// <summary>
  /// Allows to track changes in object by its <see cref="Version"/>.
  /// </summary>
  /// <typeparam name="T">The type <see cref="Version"/> property.</typeparam>
  public interface IHasVersion<T>: IHasVersion
  {
    /// <summary>
    /// Gets object version.
    /// Object isn't changed, while its
    /// <c>oldVersion.Equals(newVersion)</c>.
    /// </summary>
    new T Version { get; }
  }
}
