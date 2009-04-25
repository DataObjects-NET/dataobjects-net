// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.11

using System;
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
  public class SqlCachingKeyGenerator<TFieldType> : CachingKeyGenerator<TFieldType>
  {
    private SqlScalarRequest nextRequest;

    private readonly ISqlCompileUnit sqlNext;

    private readonly ISqlCompileUnit sqlInitialize;

    /// <inheritdoc/>
    protected override void CacheNext()
    {
      var result = new List<TFieldType>(FetchNext());
      foreach (TFieldType value in result)
        Cache.Enqueue(value);
    }

    /// <inheritdoc/>
    protected virtual IEnumerable<TFieldType> FetchNext()
    {
      TFieldType upperBound;
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      using (domainHandler.OpenSession(SessionType.System)) {
        using (var t = Session.Current.OpenTransaction()) {
          var sessionHandler = (SessionHandler) Handlers.SessionHandler;
          object value = sessionHandler.ExecuteScalarRequest(nextRequest);
          upperBound = (TFieldType)Convert.ChangeType(value, typeof (TFieldType));
          t.Complete();
        }
      }

      TFieldType current = upperBound;
      var results = new List<TFieldType>(CacheSize);
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
      if (sqlInitialize!=null) {
        var sessionHandler = (SessionHandler) Handlers.SessionHandler;
        sessionHandler.ExecuteNonQuery(sqlInitialize);
      }
    }

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="generatorInfo">The <see cref="generatorInfo"/> instance that describes generator.</param>
    /// <param name="sqlNext">The <see cref="ISqlCompileUnit"/> statement that will be used for fetching next portion of unique values from database.</param>
    public SqlCachingKeyGenerator(GeneratorInfo generatorInfo, ISqlCompileUnit sqlNext)
      : this(generatorInfo, sqlNext, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="generatorInfo">The <see cref="generatorInfo"/> instance that describes generator.</param>
    /// <param name="sqlNext">The <see cref="ISqlCompileUnit"/> statement that will be used for fetching next portion of unique values from database.</param>
    /// <param name="sqlInitialize">The <see cref="ISqlCompileUnit"/> statement that will be used for initializing sequence in database (if necessary).</param>
    public SqlCachingKeyGenerator(GeneratorInfo generatorInfo, ISqlCompileUnit sqlNext, ISqlCompileUnit sqlInitialize)
      : base(generatorInfo)
    {
      this.sqlNext = sqlNext;
      this.sqlInitialize = sqlInitialize;
    }
  }
}