// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.19

using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Providers.Sql.Servers.PostgreSql
{
  public class SqlRequestBuilder : Sql.SqlRequestBuilder
  {
    /// <inheritdoc/>
    protected override int GetExpectedResult(SqlBatch request)
    {
      return 1;
    }
  }
}