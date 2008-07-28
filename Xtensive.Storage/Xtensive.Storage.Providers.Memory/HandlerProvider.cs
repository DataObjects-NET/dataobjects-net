// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.02

namespace Xtensive.Storage.Providers.Memory
{
  [HandlerProvider("memory", Description = "General storage provider for in-memory storages.")]
  public class HandlerProvider : Providers.HandlerProvider
  {
    // Constructors

    /// <inheritdoc/>
    public HandlerProvider(Domain domain)
      : base(domain)
    {
    }
  }
}