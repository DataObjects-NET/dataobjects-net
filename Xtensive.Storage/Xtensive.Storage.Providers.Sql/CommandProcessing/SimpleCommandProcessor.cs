// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Sql
{
  internal class SimpleCommandProcessor : CommandProcessor
  {
    public override void ExecuteRequests(bool dirty)
    {
      foreach (var persistTask in persistTasks)
        ExecutePersist(persistTask);
      persistTasks.Clear();
      foreach (var queryTask in queryTasks)
        ReadTuples(ExecuteQuery(queryTask), queryTask.Request.TupleDescriptor, queryTask.Output);
      queryTasks.Clear();
    }

    private void ReadTuples(DbDataReader reader, TupleDescriptor descriptor, List<Tuple> output)
    {
      var enumerator = RunTupleReader(reader, descriptor);
      using (enumerator) {
        while (enumerator.MoveNext())
          output.Add(enumerator.Current);
      }
    }
    
    public override IEnumerator<Tuple> ExecuteRequestsWithReader(SqlQueryRequest lastRequest)
    {
      ExecuteRequests(false);
      return RunTupleReader(ExecuteQuery(new SqlQueryTask(lastRequest)), lastRequest.TupleDescriptor);
    }

    // Constructors

    public SimpleCommandProcessor(DomainHandler domainHandler)
      : base(domainHandler)
    {
    }
  }
}