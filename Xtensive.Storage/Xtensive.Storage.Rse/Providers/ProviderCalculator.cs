// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.07

using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers
{
  public abstract class ProviderCalculator : ProviderImplementation
  {
    private IEnumerable<Tuple> calculatedResults;

    /// <summary>
    /// Ensures that <see cref="Calculate"/> method has been called once.
    /// </summary>
    public void EnsureIsCalculated()
    {
      if (calculatedResults == null) lock (this) if (calculatedResults == null)
        calculatedResults = Calculate();
    }

    /// <summary>
    /// Calculates provider's result.
    /// </summary>
    protected abstract IEnumerable<Tuple> Calculate();

    /// <summary>
    /// Sets calculation flag to default (unitialized) state.
    /// </summary>
    public void Reset()
    {
      calculatedResults = null;
    }

    /// <inheritdoc/>
    public sealed override IEnumerator<Tuple> GetEnumerator()
    {
      EnsureIsCalculated();
      return calculatedResults.GetEnumerator();
    }


    // Constructor

    public ProviderCalculator(RecordHeader header, params Provider[] sourceProviders)
      : base(header, sourceProviders)
    {
    }
  }
}