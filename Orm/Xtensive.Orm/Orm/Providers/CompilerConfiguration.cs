// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

namespace Xtensive.Orm.Providers
{
  public sealed class CompilerConfiguration
  {
    public bool PrepareRequest { get; set; }

    internal StorageNode StorageNode { get; set; }


    // Constructors

    public CompilerConfiguration()
    {
      PrepareRequest = true;
    }
  }
}