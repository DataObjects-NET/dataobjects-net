// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers
{
  public abstract class Provider : IEnumerable<Tuple>
  {
    private RecordHeader header;

    public RecordHeader Header
    {
      get
      {
        if (header == null) lock (this) if (header == null)
          header = BuildHeader();
        return header;
      }
    }

    public Provider[] SourceProviders { get; private set; }

    public virtual ProviderOptionsStruct Options
    {
      get { return ProviderOptions.Default; }
    }

    public virtual T GetService<T>() where T : class
    {
      return this as T;
    }

    public T GetService<T>(bool throwException) where T: class
    {
      var service = GetService<T>();
      if (throwException && service == null)
        throw new InvalidOperationException();
      return service;
    }

    protected abstract RecordHeader BuildHeader();

    public abstract IEnumerator<Tuple> GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructor

    protected Provider(params Provider[] sourceProviders)
    {
      SourceProviders = sourceProviders;
    }
  }
}