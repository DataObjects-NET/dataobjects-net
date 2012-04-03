// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.07

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Providers
{
  internal sealed class SequenceQuery
  {
    public string InsertQuery { get; private set; }

    public string SelectQuery { get; private set; }

    public bool RequiresSeparateSession { get; private set; }

    public long ExecuteWith(ISqlExecutor sqlExecutor)
    {
      if (InsertQuery!=null)
        sqlExecutor.ExecuteNonQuery(InsertQuery);
      return Convert.ToInt64(sqlExecutor.ExecuteScalar(SelectQuery));
    }

    // Constructors

    public SequenceQuery(string insertQuery, string selectQuery, bool requiresSeparateSession)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(insertQuery, "insertQuery");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(selectQuery, "selectQuery");

      InsertQuery = insertQuery;
      SelectQuery = selectQuery;
      RequiresSeparateSession = requiresSeparateSession;
    }

    public SequenceQuery(string selectQuery, bool requiresSeparateSession)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(selectQuery, "selectQuery");
      SelectQuery = selectQuery;
      RequiresSeparateSession = requiresSeparateSession;
    }
  }
}