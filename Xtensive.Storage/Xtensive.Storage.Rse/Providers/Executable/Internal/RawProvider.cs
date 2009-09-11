// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public sealed class RawProvider : ExecutableProvider<Compilable.RawProvider>,
    IListProvider
  {
    #region Cached properties

    private const string CachedSourceName = "CachedSource";

    private Tuple[] CachedSource {
      get { return GetCachedValue<Tuple[]>(EnumerationContext.Current, CachedSourceName); }
      set { SetCachedValue(EnumerationContext.Current, CachedSourceName, value); }
    }

    #endregion

    public long Count
    {
      get { return CachedSource.Length; }
    }

    public Tuple GetItem(int index)
    {
      return CachedSource[index];
    }

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedSource = Origin.CompiledSource.Invoke();
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return CachedSource;
    }


    // Constructors

    public RawProvider(Compilable.RawProvider origin)
      : base(origin)
    {
      AddService<IListProvider>();
    }
  }
}