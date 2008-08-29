// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class LoadPovider : ExecutableProvider<Compilable.LoadProvider>,
    IHasNamedResult
  {
    #region Cached properties

    private const string CachedNameName = "CachedName";
    private const string CachedResultName = "CachedResult";

    private string CachedName
    {
      get { return GetCachedValue<string>(EnumerationContext.Current, CachedNameName); }
      set { SetCachedValue(EnumerationContext.Current, CachedNameName, value); }
    }

    private IEnumerable<Tuple> CachedResult
    {
      get { return GetCachedValue<IEnumerable<Tuple>>(EnumerationContext.Current, CachedResultName); }
      set { SetCachedValue(EnumerationContext.Current, CachedResultName, value); }
    }

    #endregion

    public TemporaryDataScope Scope {
      get { return Origin.Scope; }
    }

    public string Name {
      get {
        var name = Origin.Name;
        return name.IsNullOrEmpty() ? CachedName : name;
      }
    }

    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedName = Origin.Name;
      CachedResult = (IEnumerable<Tuple>) DomainLevelTemporaryData.Current.Get(Name);
    }

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      foreach (var tuple in CachedResult)
        yield return tuple;
    }

    protected internal override void OnAfterEnumerate(EnumerationContext context)
    {
      if (Origin.Scope==TemporaryDataScope.Enumeration)
        DomainLevelTemporaryData.Current.Set(Name, null);
      base.OnAfterEnumerate(context);
    }


    // Constructors

    public LoadPovider(Compilable.LoadProvider origin)
      : base(origin)
    {
    }
  }
}