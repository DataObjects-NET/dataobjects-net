// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.31

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Public API to <see cref="Persistent"/> instance cache 
  /// (see <see cref="Persistent.Cache">Persistent.Cache</see>).
  /// </summary>
  public struct PersistentCacheAccessor
  {
    private Persistent persistent;

    /// <summary>
    /// Gets the <see cref="Persistent"/> instance this cache is bound to.
    /// </summary>
    public Persistent Persistent
    {
      get { return persistent; }
    }

    /// <summary>
    /// Gets the state of the field.
    /// </summary>
    /// <param name="fieldName">Name of the field.</param>
    /// <returns>The state of the field.</returns>
    public PersistentFieldState GetFieldState(string fieldName)
    {
      return persistent.GetFieldState(fieldName);
    }

    /// <summary>
    /// Gets the state of the field.
    /// </summary>
    /// <param name="field">The field to get the state for.</param>
    /// <returns>The state of the field.</returns>
    /// <exception cref="ArgumentException"><paramref name="field"/> belongs to a different type.</exception>
    public PersistentFieldState GetFieldState(FieldInfo field)
    {
      return persistent.GetFieldState(field);
    }


    // Constructors

    internal PersistentCacheAccessor(Persistent persistent)
    {
      this.persistent = persistent;
    }
  }
}