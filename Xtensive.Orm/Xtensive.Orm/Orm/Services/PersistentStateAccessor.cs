// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.31

using System;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Public API to cached state of <see cref="Persistent"/> instance
  /// (see <see cref="DirectStateAccessor"/>).
  /// </summary>
  public struct PersistentStateAccessor
  {
    private readonly Persistent persistent;

    /// <summary>
    /// Gets the <see cref="Persistent"/> instance this accessor is bound to.
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

    internal PersistentStateAccessor(Persistent persistent)
    {
      this.persistent = persistent;
    }
  }
}