// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System.Collections.Generic;

namespace Xtensive.Orm.Providers
{
  public sealed class CompilerConfiguration
  {
    public bool PrepareRequest { get; set; }
    public bool PreferTypeIdAsParameter { get; set; }
    public IReadOnlyList<string> Tags { get; init; }

    internal StorageNode StorageNode { get; set; }


    // Constructors

    public CompilerConfiguration()
    {
      PrepareRequest = true;
    }
  }
}