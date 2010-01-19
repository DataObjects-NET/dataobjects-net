// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.11

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Caching generator implementation for SQL-based storages.
  /// </summary>
  /// <typeparam name="TFieldType">The type of the field.</typeparam>
  public class SqlCachingKeyGenerator<TFieldType> : CachingKeyGenerator<TFieldType>
  {
    private string insertRequest;
    private string selectRequest;
    private ISqlCompileUnit sqlNext;
    private ISqlCompileUnit sqlInitialize;

    /// <inheritdoc/>
    protected override void CacheNext()
    {
      foreach (var item in FetchNext())
        Cache.Enqueue(item);
    }

    private IEnumerable<TFieldType> FetchNext()
    {
      TFieldType upperBound;
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      using (Session.Open(domainHandler.Domain, SessionType.KeyGenerator))
      using (var t = Transaction.Open()) {
        var executor = Handlers.SessionHandler.GetService<IQueryExecutor>();
        if (!string.IsNullOrEmpty(insertRequest))
          executor.ExecuteNonQuery(insertRequest);
        object value = executor.ExecuteScalar(selectRequest);
        upperBound = (TFieldType) Convert.ChangeType(value, typeof (TFieldType));
        // rolling back transaction
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
      
      if (sqlInitialize!=null) {
        var sessionHandler = Handlers.SessionHandler.GetService<IQueryExecutor>();
        sessionHandler.ExecuteNonQuery(sqlInitialize);
        sqlInitialize = null;
      }

      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      var batch = sqlNext as SqlBatch;
      if (batch != null && !domainHandler.ProviderInfo.Supports(ProviderFeatures.Batches)) {
        insertRequest = domainHandler.Driver.Compile((ISqlCompileUnit) batch[0]).GetCommandText();
        selectRequest = domainHandler.Driver.Compile((ISqlCompileUnit) batch[1]).GetCommandText();
      }
      else
        selectRequest = domainHandler.Driver.Compile(sqlNext).GetCommandText();
      sqlNext = null;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyProviderInfo">The <see cref="KeyProviderInfo"/> instance that describes generator.</param>
    /// <param name="sqlNext">The <see cref="ISqlCompileUnit"/> statement that will be used for fetching next portion of unique values from database.</param>
    public SqlCachingKeyGenerator(KeyProviderInfo keyProviderInfo, ISqlCompileUnit sqlNext)
      : this(keyProviderInfo, sqlNext, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyProviderInfo">The <see cref="KeyProviderInfo"/> instance that describes generator.</param>
    /// <param name="sqlNext">The <see cref="ISqlCompileUnit"/> statement that will be used for fetching next portion of unique values from database.</param>
    /// <param name="sqlInitialize">The <see cref="ISqlCompileUnit"/> statement that will be used for initializing sequence in database (if necessary).</param>
    public SqlCachingKeyGenerator(KeyProviderInfo keyProviderInfo, ISqlCompileUnit sqlNext, ISqlCompileUnit sqlInitialize)
      : base(keyProviderInfo)
    {
      this.sqlNext = sqlNext;
      this.sqlInitialize = sqlInitialize;
    }
  }
}