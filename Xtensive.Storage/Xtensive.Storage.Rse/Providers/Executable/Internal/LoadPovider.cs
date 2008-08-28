// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class LoadPovider : ExecutableProvider<Compilable.LoadProvider>
  {
    #region Cached properties

    private const string CachedSourceName = "CachedSource";

    private IEnumerable<Tuple> CachedSource
    {
      get { return GetCachedValue<IEnumerable<Tuple>>(EnumerationContext.Current, CachedSourceName); }
      set { SetCachedValue(EnumerationContext.Current, CachedSourceName, value); }
    }

    #endregion

    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      Compilable.LoadProvider loadProvider = Origin;
      CachedSource = (IEnumerable<Tuple>) DomainSavedData.Current.Get(loadProvider.ResultName);
    }

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return CachedSource;
    }


    // Constructor.

    public LoadPovider(Compilable.LoadProvider origin)
      : base(origin)
    {
    }
  }
}