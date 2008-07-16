// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;

namespace Xtensive.Storage.Rse.Providers
{
  [Serializable]
  public class AliasProvider : CompilableProvider
  {
    public CompilableProvider Source { get; private set; }
    public string Alias { get; private set; }

    protected override RecordHeader BuildHeader()
    {
      return new RecordHeader(Source.Header, Alias);
    }

    // Constructor

    public AliasProvider(CompilableProvider provider, string alias)
      : base(provider)
    {
      Source = provider;
      Alias = alias;
    }
  }
}