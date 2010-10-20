// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System;
using Xtensive.Core;

namespace Xtensive.Caching
{
  /// <summary>
  /// Describes expiring <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/>.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/>.</typeparam>
  public sealed class Expiring<T> : CachedValueBase<T, DateTime>
  {
    private readonly Func<T, DateTime, Pair<T, DateTime>> calculator;

    /// <inheritdoc/>
    public override bool IsActual {
      get { return State.Second >= DateTime.UtcNow; }
    }

    /// <inheritdoc/>
    protected override void Refresh()
    {
      State = calculator.Invoke(State.First, State.Second);
    }

    
    // Constructors

    /// <inheritdoc/>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(Func<T, DateTime, Pair<T, DateTime>> calculator)
    {
      this.calculator = calculator;
    }

    /// <inheritdoc/>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(Func<Pair<T, DateTime>> calculator)
    {
      this.calculator = (_v,_t) => calculator();
    }

    /// <inheritdoc/>
    /// <param name="expiresIn">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> expiration period.</param>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(TimeSpan expiresIn, Func<T> calculator)
    {
      this.calculator = (_v,_t) => new Pair<T, DateTime>(calculator(), DateTime.UtcNow + expiresIn);
    }

    /// <inheritdoc/>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(object syncRoot, Func<T, DateTime, Pair<T, DateTime>> calculator)
      : base(syncRoot)
    {
      this.calculator = calculator;
    }

    /// <inheritdoc/>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(object syncRoot, Func<Pair<T, DateTime>> calculator)
      : base(syncRoot)
    {
      this.calculator = (_v,_t) => calculator();
    }

    /// <inheritdoc/>
    /// <param name="expiresIn">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> expiration period.</param>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(object syncRoot, TimeSpan expiresIn, Func<T> calculator)
      : base(syncRoot)
    {
      this.calculator = (_v,_t) => new Pair<T, DateTime>(calculator(), DateTime.UtcNow + expiresIn);
    }

    /// <inheritdoc/>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(bool syncOnItself, Func<T, DateTime, Pair<T, DateTime>> calculator)
      : base(syncOnItself)
    {
      this.calculator = calculator;
    }

    /// <inheritdoc/>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(bool syncOnItself, Func<Pair<T, DateTime>> calculator)
      : base(syncOnItself)
    {
      this.calculator = (_v,_t) => calculator();
    }

    /// <inheritdoc/>
    /// <param name="expiresIn">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> expiration period.</param>
    /// <param name="calculator">The <see cref="CachedValueBase{TValue,TActualizationInfo}.Value"/> calculator.</param>
    public Expiring(bool syncOnItself, TimeSpan expiresIn, Func<T> calculator)
      : base(syncOnItself)
    {
      this.calculator = (_v,_t) => new Pair<T, DateTime>(calculator(), DateTime.UtcNow + expiresIn);
    }
  }
}