// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal class StoreProvider : UnaryExecutableProvider<Compilable.StoreProvider>,
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

    public override void OnBeforeEnumerate(EnumerationContext context)
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

    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var result = CachedResult;
      foreach (var tuple in result)
        yield return tuple;
    }
   

    // Constructors

    public StoreProvider(Compilable.StoreProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<IHasNamedResult>();
      if (Scope==TemporaryDataScope.Global)
        namedValues = GlobalTemporaryData.Current;
      else
        namedValues = TransactionTemporaryData.Current;
    }

    public StoreProvider(Compilable.StoreProvider origin)
      : this(origin, null)
    {
    }
  }
}