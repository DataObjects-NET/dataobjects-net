// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal class SaveProvider : UnaryExecutableProvider<Compilable.SaveProvider>,
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
      string name = Origin.Name;
      if (name.IsNullOrEmpty())
        name = GenerateTemporaryName();
      CachedName = name;
    }
    
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var list = Source.ToList();
      // TODO: Fix this!
      DomainLevelTemporaryData.Current.Set(Name, list);
      foreach (var tuple in list)
        yield return tuple;
    }

    #region Private \ internal methods

    private static string GenerateTemporaryName()
    {
      return Guid.NewGuid().ToString();
    }

    #endregion


    // Constructor.

    public SaveProvider(Compilable.SaveProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<IHasNamedResult>();
    }
  }
}