// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

namespace Xtensive.Storage.Providers.MsSql
{
  [Provider("sqlserver", Description = "General storage provider for MS SQL 2005 based storages.")]
  public class HandlerFactory : Providers.HandlerFactory
  {
    // Constructors

    /// <inheritdoc/>
    public HandlerFactory(Domain domain)
      : base(domain)
    {
    }
  }
}