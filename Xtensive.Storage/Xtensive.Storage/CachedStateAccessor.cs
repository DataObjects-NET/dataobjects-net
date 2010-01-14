// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.14

namespace Xtensive.Storage
{
  public static class CachedStateAccessor
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
    public static PersistentCacheAccessor Get(Persistent source) 
    {
      return new PersistentCacheAccessor(source);
    }
    
    /// <summary>
    /// Gets public accessor to <see cref="EntitySetBase"/> instance state.
    /// </summary>
    public static EntitySetCacheAccessor Get(EntitySetBase source) 
    {
      return new EntitySetCacheAccessor(source);
    }
  }
}