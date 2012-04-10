// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.24

using System;


namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Acquires the lock for a data in a source provider.
  /// </summary>
  [Serializable]
  public sealed class LockProvider : UnaryProvider
  {
    /// <summary>
    /// The delegate returning the mode of the lock to be acquired.
    /// </summary>
    public readonly Func<LockMode> LockMode;

    /// <summary>
    /// The delegate returning the behavior of the lock.
    /// </summary>
    public readonly Func<LockBehavior> LockBehavior;


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="lockMode">The mode of the lock to be acquired.</param>
    /// <param name="lockBehavior">The behavior of the lock.</param>
    public LockProvider(CompilableProvider source, LockMode lockMode, LockBehavior lockBehavior) :
      base(ProviderType.Lock, source)
    {
      LockMode = () => lockMode;
      LockBehavior = () => lockBehavior;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="lockMode">The delegate returning the mode of the lock to be acquired.</param>
    /// <param name="lockBehavior">The delegate returning the behavior of the lock.</param>
    public LockProvider(CompilableProvider source, Func<LockMode> lockMode, Func<LockBehavior> lockBehavior) :
      base(ProviderType.Lock, source)
    {
      LockMode = lockMode;
      LockBehavior = lockBehavior;
    }
  }
}