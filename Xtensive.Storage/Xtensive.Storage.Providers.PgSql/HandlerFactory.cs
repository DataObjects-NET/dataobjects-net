// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.15

namespace Xtensive.Storage.Providers.PgSql
{
  [Provider("pgsql", Description = "Storage provider for PostgreSQL.")]
  [Provider("postgres", Description = "Storage provider for PostgreSQL.")]
  [Provider("postgresql", Description = "Storage provider for PostgreSQL.")]
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