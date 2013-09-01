// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.29

using Xtensive.Core;

namespace Xtensive.Orm.Rse.Providers
{
  public sealed class VoidProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;

    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }

    public VoidProvider(RecordSetHeader header)
      : base(ProviderType.Void)
    {
      ArgumentValidator.EnsureArgumentNotNull(header, "header");
      this.header = header;
      Initialize();
    }
  }
}