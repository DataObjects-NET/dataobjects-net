// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System;
using Xtensive.Caching;


namespace Xtensive.Orm
{
  /// <summary>
  /// Transactional value cache.
  /// </summary>
  /// <typeparam name="T">The type of the <see cref="Value"/>.</typeparam>
  public sealed class TransactionalValue<T> : TransactionalStateContainer<T>,
    IInvalidatable
  {
    private readonly Func<T> calculator;

    /// <summary>
    /// Gets the cached value.
    /// If it isn't valid anymore (see <see cref="TransactionalStateContainer{TState}.IsActual"/>), 
    /// it gets re-calculated.
    /// </summary>
    public T Value {
      get {
        return State;
      }
    }

    /// <see cref="IInvalidatable.Invalidate" copy="true"/>
    public new void Invalidate()
    {
      base.Invalidate();
    }

    /// <inheritdoc/>
    protected override void Refresh()
    {
      State = calculator();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session to bind this object to.</param>
    /// <param name="calculator">The delegate calculating a new value.</param>
    public TransactionalValue(Session session, Func<T> calculator)
      : base(session)
    {
      this.calculator = calculator;
    }
  }
}