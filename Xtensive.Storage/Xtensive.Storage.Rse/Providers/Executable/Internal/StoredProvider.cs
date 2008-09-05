// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal class StoredProvider : UnaryExecutableProvider<Compilable.StoredProvider>,
    IHasNamedResult
  {
    private readonly NamedValueCollection namedValues;

    #region Cached properties

    private const string CachedResultName = "CachedResult";

    private IEnumerable<Tuple> CachedResult
    {
      get { return GetCachedValue<IEnumerable<Tuple>>(EnumerationContext.Current, CachedResultName); }
      set { SetCachedValue(EnumerationContext.Current, CachedResultName, value); }
    }

    #endregion

    /// <inheritdoc/>
    public TemporaryDataScope Scope
    {
      get { return Origin.Scope; }
    }

    /// <inheritdoc/>
    public string Name
    {
      get { return Origin.Name; }
    }

    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      List<Tuple> result;
      if (Source!=null) {
        result = Source.ToList();
        namedValues.Set(Name, result.Count!=0 ? result : null);
      }
      else
        result = (List<Tuple>) namedValues.Get(Name);
      CachedResult = result ?? new List<Tuple>();
    }

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var result = CachedResult;
      foreach (var tuple in result)
        yield return tuple;
    }

    protected internal override void OnAfterEnumerate(EnumerationContext context)
    {
      if (Origin.Scope==TemporaryDataScope.Enumeration)
        GlobalTemporaryData.Current.Set(Name, null);
      base.OnAfterEnumerate(context);
    }


    // Constructors

    public StoredProvider(Compilable.StoredProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<IHasNamedResult>();
      if (Scope==TemporaryDataScope.Global)
        namedValues = GlobalTemporaryData.Current;
      else
        namedValues = TransactionTemporaryData.Current;
    }

    public StoredProvider(Compilable.StoredProvider origin)
      : this(origin, null)
    {
    }
  }
}