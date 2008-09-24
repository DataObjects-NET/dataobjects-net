// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.19

namespace Xtensive.Storage.Providers.PgSql
{
  public class SqlRequestBuilder : Sql.SqlRequestBuilder
  {
    /// <inheritdoc/>
    protected override void  SetExpectedResult(Sql.SqlModificationRequest request)
    {
      request.ExpectedResult = 1;
    }
  }
}