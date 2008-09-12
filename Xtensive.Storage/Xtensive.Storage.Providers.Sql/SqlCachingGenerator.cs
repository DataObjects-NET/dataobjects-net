// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.11

using System.Collections.Generic;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlCachingGenerator<TFieldType> : CachingGenerator<TFieldType>
  {
    private SqlScalarRequest nextRequest;
    private readonly ISqlCompileUnit sqlNext;
    private readonly ISqlCompileUnit sqlCreate;

    /// <inheritdoc/>
    protected override void CacheNext()
    {
      List<TFieldType> result = new List<TFieldType>(LoadNext());
      foreach (TFieldType value in result)
        Cache.Enqueue(value);
    }

    /// <inheritdoc/>
    protected virtual IEnumerable<TFieldType> LoadNext()
    {
      TFieldType upperBound;
      DomainHandler dh = (DomainHandler) Handlers.DomainHandler;
      using (dh.OpenSession(SessionType.System)) {
        using (Session.Current.OpenTransaction()) {
          Sql.SessionHandler sh = (SessionHandler) Handlers.SessionHandler;
          upperBound = (TFieldType) sh.ExecuteScalar(nextRequest);
        }
      }

      TFieldType current = upperBound;
      List<TFieldType> results = new List<TFieldType>(CacheSize);
      for (int i = 0; i < CacheSize; i++) {
        results.Add(current);
        current = Arithmetic.Subtract(current, Arithmetic.One);
      }
      results.Reverse();
      return results;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();

      nextRequest = new SqlScalarRequest(sqlNext);
      DomainHandler dh = (DomainHandler) Handlers.DomainHandler;
      dh.Compile(nextRequest);

      if (sqlCreate!=null) {
        Sql.SessionHandler sh = (SessionHandler) Handlers.SessionHandler;
        sh.ExecuteNonQuery(sqlCreate);
      }
    }


    // Constructor

    public SqlCachingGenerator(HierarchyInfo hierarchy, int cacheSize, ISqlCompileUnit sqlNext, ISqlCompileUnit sqlCreate)
      : base(hierarchy, cacheSize)
    {
      this.sqlNext = sqlNext;
      this.sqlCreate = sqlCreate;
    }
  }
}