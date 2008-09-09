// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.08

namespace Xtensive.Storage.Providers.VistaDb
{
  [Provider("vistadb", Description = "Storage provider for VistaDb.")]
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