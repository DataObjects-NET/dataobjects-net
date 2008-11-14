// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.05

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to core services bound to a <see cref="Session"/>.
  /// </summary>
  public sealed class CoreServiceAccessor : SessionBound
  {
    /// <summary>
    /// Gets the accessor for <see cref="Persistent"/> descendants.
    /// </summary>
    public PersistentAccessor PersistentAccessor { get; private set; }


    // Constructors

    /// <inheritdoc/>
    public CoreServiceAccessor(Session session)
      : base(session)
    {
      PersistentAccessor = new PersistentAccessor(session);
    }
  }
}