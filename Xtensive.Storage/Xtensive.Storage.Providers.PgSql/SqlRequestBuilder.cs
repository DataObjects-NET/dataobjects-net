// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.19

namespace Xtensive.Storage.Providers.PgSql
{
  public class SqlRequestBuilder : Sql.SqlRequestBuilder
  {
    protected override Sql.SqlModificationRequest CreateRequest(Sql.SqlRequestBuilderResult result)
    {
      Sql.SqlModificationRequest request = base.CreateRequest(result);
      request.ExpectedResult = 1;
      return request;
    }
  }
}