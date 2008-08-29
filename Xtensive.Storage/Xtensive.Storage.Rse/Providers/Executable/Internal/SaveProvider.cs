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
      if (Scope==TemporaryDataScope.Global)
        GlobalTemporaryData.Current.Set(Name, list.Count!=0 ? list : null);
      else
        TransactionTemporaryData.Current.Set(Name, list.Count!=0 ? list : null);
      foreach (var tuple in list)
        yield return tuple;
    }

    protected internal override void OnAfterEnumerate(EnumerationContext context)
    {
      if (Origin.Scope==TemporaryDataScope.Enumeration)
        GlobalTemporaryData.Current.Set(Name, null);
      base.OnAfterEnumerate(context);
    }

    #region Private \ internal methods

    private static string GenerateTemporaryName()
    {
      return Guid.NewGuid().ToString();
    }

    #endregion


    // Constructors

    public SaveProvider(Compilable.SaveProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<IHasNamedResult>();
    }
  }
}