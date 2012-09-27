// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Enumerates specified array of <see cref="Tuple"/> instances.
  /// </summary>
  [Serializable]
  public sealed class ExecutableRawProvider : ExecutableProvider<Providers.RawProvider>
  {
    #region Cached properties

    private const string CachedSourceName = "CachedSource";

    private IEnumerable<Tuple> CachedSource {
      get { return GetValue<IEnumerable<Tuple>>(EnumerationContext.Current, CachedSourceName); }
      set { SetValue(EnumerationContext.Current, CachedSourceName, value); }
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedSource = Origin.CompiledSource.Invoke();
    }

    /// <inheritdoc/>
    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return CachedSource;
    }


    // Constructors

    public ExecutableRawProvider(Providers.RawProvider origin)
      : base(origin)
    {
    }
  }
}