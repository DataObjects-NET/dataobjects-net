// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.16

using System;
using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Executable provider that makes decision which real executable provider to use.
  /// </summary>
  /// <typeparam name="TOrigin">Compilable provider.</typeparam>
  [Serializable]
  public abstract class SubstitutingProvider<TOrigin> : ExecutableProvider<TOrigin>
    where TOrigin: CompilableProvider
  {
    /// <summary>
    /// Gets the real provider used by this class.
    /// </summary>
    public ExecutableProvider<TOrigin> Substitution { get; private set; }

    /// <summary>
    /// Builds the <see cref="Substitution"/>.
    /// </summary>
    public abstract ExecutableProvider<TOrigin> BuildSubstitution();

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      return Substitution.GetService<T>();
    }

    /// <inheritdoc/>
    public override void OnBeforeEnumerate(EnumerationContext context)
    {
      Substitution.OnBeforeEnumerate(context);
    }

    /// <inheritdoc/>
    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Substitution.Enumerate(context);
    }

    /// <inheritdoc/>
    public override void OnAfterEnumerate(EnumerationContext context)
    {
      Substitution.OnAfterEnumerate(context);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      Substitution = BuildSubstitution();
    }

   
    // Constructors

    protected SubstitutingProvider(TOrigin origin, params ExecutableProvider[] sourceProviders)
      : base(origin, sourceProviders)
    {
    }
  }
}