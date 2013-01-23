// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.01.23

using System;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Accessor for tuple fields.
  /// </summary>
  public abstract class TupleFieldAccessor
  {
    /// <summary>
    /// Getter delegate.
    /// </summary>
    protected Delegate Getter;

    /// <summary>
    /// Setter delegate.
    /// </summary>
    protected Delegate Setter;

    /// <summary>
    /// Nullable getter delegate.
    /// </summary>
    protected Delegate NullableGetter;

    /// <summary>
    /// Nullable setter delegate.
    /// </summary>
    protected Delegate NullableSetter;

    /// <summary>
    /// Gets setter for the field.
    /// </summary>
    /// <typeparam name="T">Field type.</typeparam>
    /// <param name="isNullable">Flag indicating if field type is nullable.</param>
    /// <returns><see cref="GetValueDelegate{TValue}"/> for the field.</returns>
    public GetValueDelegate<T> GetGetter<T>(bool isNullable)
    {
      return (isNullable ? NullableGetter : Getter) as GetValueDelegate<T>;
    }

    /// <summary>
    /// Gets setter for the field.
    /// </summary>
    /// <typeparam name="T">Field type.</typeparam>
    /// <param name="isNullable">Flag indicating if field type is nullable.</param>
    /// <returns><see cref="SetValueDelegate{TValue}"/> for the field.</returns>
    public SetValueDelegate<T> GetSetter<T>(bool isNullable)
    {
      return (isNullable ? NullableSetter : Setter) as SetValueDelegate<T>;
    }
  }
}