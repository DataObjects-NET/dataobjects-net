// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal class SaveProvider : ExecutableProvider<Compilable.SaveProvider>,
    IProvideNamedResult
  {
    private readonly Compilable.SaveProvider saveProvider;
    private readonly Provider source;
    private readonly string resultName;

    #region Cached properties

    private const string CachedResultSourceName = "CachedResultName";

    private string CachedResultName
    {
      get { return (string)GetCachedValue<object>(EnumerationContext.Current, CachedResultSourceName); }
      set { SetCachedValue(EnumerationContext.Current, CachedResultSourceName, (object)value); }
    }

    #endregion

    public string GetResultName()
    {
      return resultName;
    }

    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedResultName = resultName;
    }
    
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var list = source.ToList();
      DomainSavedData.Current.Set(CachedResultName, list);
      return list;
    }

    private static string GenerateResultName(string resultName)
    {
      if (!string.IsNullOrEmpty(resultName))
        return resultName;
      var random = new Random();
      return random.Next().ToString();
    }


    // Constructor.

    public SaveProvider(Compilable.SaveProvider origin, ExecutableProvider source)
      : base(origin,source)
    {
      saveProvider = origin;
      this.source = source;
      resultName = GenerateResultName(saveProvider.ResultName);
      AddService<IProvideNamedResult>();

    }
  }
}