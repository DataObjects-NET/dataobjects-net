// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.14

namespace Xtensive.Storage.Services
{
  /// <summary>
  /// Provides direct read-only access to various caches.
  /// Note that this is a fully static service.
  /// </summary>
  public static class DirectStateAccessor
  {
    /// <summary>
    /// Gets public accessor to <see cref="Session"/> state.
    /// </summary>
    public static SessionStateAccessor Get(Session source) 
    {
      return new SessionStateAccessor(source);
    }

    /// <summary>
    /// Gets public accessor to <see cref="Persistent"/> instance state.
    /// </summary>
    public static PersistentStateAccessor Get(Persistent source) 
    {
      return new PersistentStateAccessor(source);
    }
    
    /// <summary>
    /// Gets public accessor to <see cref="EntitySetBase"/> instance state.
    /// </summary>
    public static EntitySetStateAccessor Get(EntitySetBase source) 
    {
      return new EntitySetStateAccessor(source);
    }
  }
}