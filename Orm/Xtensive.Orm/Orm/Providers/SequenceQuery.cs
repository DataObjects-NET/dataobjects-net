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

    public string DeleteQuery { get; private set; }

    public SequenceQueryCompartment Compartment { get; private set; }

    public long ExecuteWith(ISqlExecutor sqlExecutor)
    {
      if (DeleteQuery != null)
        sqlExecutor.ExecuteNonQuery(DeleteQuery);
      if (InsertQuery!=null)
        sqlExecutor.ExecuteNonQuery(InsertQuery);
      return Convert.ToInt64(sqlExecutor.ExecuteScalar(SelectQuery));
    }

    // Constructors

    public SequenceQuery(string deleteQuery, string insertQuery, string selectQuery, SequenceQueryCompartment compartment)
    {
      ArgumentValidator.EnsureArgumentNotNull(deleteQuery, "deleteQuery");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(insertQuery, "insertQuery");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(selectQuery, "selectQuery");

      DeleteQuery = deleteQuery;
      InsertQuery = insertQuery;
      SelectQuery = selectQuery;
      Compartment = compartment;
    }

    public SequenceQuery(string insertQuery, string selectQuery, SequenceQueryCompartment compartment)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(insertQuery, "insertQuery");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(selectQuery, "selectQuery");

      InsertQuery = insertQuery;
      SelectQuery = selectQuery;
      Compartment = compartment;
    }

    public SequenceQuery(string selectQuery, SequenceQueryCompartment compartment)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(selectQuery, "selectQuery");
      SelectQuery = selectQuery;
      Compartment = compartment;
    }
  }
}