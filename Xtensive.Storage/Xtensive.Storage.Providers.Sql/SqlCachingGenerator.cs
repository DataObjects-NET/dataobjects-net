// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.11

using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Caching generator implementation for sql-based storages.
  /// </summary>
  /// <typeparam name="TFieldType">The type of the field.</typeparam>
  public class SqlCachingGenerator<TFieldType> : CachingGenerator<TFieldType>
  {
    private SqlScalarRequest nextRequest;
    private readonly ISqlCompileUnit sqlNext;
    private readonly ISqlCompileUnit sqlCreate;

    /// <inheritdoc/>
    protected override void CacheNext()
    {
      List<TFieldType> result = new List<TFieldType>(FetchNext());
      foreach (TFieldType value in result)
        Cache.Enqueue(value);
    }

    /// <inheritdoc/>
    protected virtual IEnumerable<TFieldType> FetchNext()
    {
      TFieldType upperBound;
      DomainHandler dh = (DomainHandler) Handlers.DomainHandler;
      using (dh.OpenSession(SessionType.System)) {
        using (var t = Session.Current.OpenTransaction()) {
          Sql.SessionHandler sh = (SessionHandler) Handlers.SessionHandler;
          upperBound = (TFieldType) sh.ExecuteScalar(nextRequest);
          t.Complete();
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy this instance will serve.</param>
    /// <param name="cacheSize">Size of the cache.</param>
    /// <param name="sqlNext">The <see cref="ISqlCompileUnit"/> statement that will be used for fetching next portion of unique values from database.</param>
    /// <param name="sqlCreate">The <see cref="ISqlCompileUnit"/> statement that will be used for underlying source of unique sequence creation in database.</param>
    public SqlCachingGenerator(HierarchyInfo hierarchy, int cacheSize, ISqlCompileUnit sqlNext, ISqlCompileUnit sqlCreate)
      : base(hierarchy, cacheSize)
    {
      this.sqlNext = sqlNext;
      this.sqlCreate = sqlCreate;
    }
  }
}