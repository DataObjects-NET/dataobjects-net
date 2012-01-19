// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Orm.Providers.Sql.Resources;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// <see cref="ICachingKeyGeneratorService"/> implementation for SQL databases.
  /// </summary>
  [Service(typeof(ICachingKeyGeneratorService), Singleton = true)]
  public class CachingKeyGeneratorService : ICachingKeyGeneratorService
  {
    private class CachingKeyGeneratorInfo
    {
      public string InsertRequest;
      public string SelectRequest;
    }

    private Dictionary<KeyGenerator, CachingKeyGeneratorInfo> cachingKeyGeneratorInfo =
      new Dictionary<KeyGenerator, CachingKeyGeneratorInfo>();

    private readonly HandlerAccessor handlers;

    /// <inheritdoc/>
    public IEnumerable<TFieldType> NextBulk<TFieldType>(CachingKeyGenerator<TFieldType> generator)
    {
      var info = GetKeyGeneratorInfo(generator);
      TFieldType hiValue;

      var isUpgradeRunning = UpgradeContext.Current != null;

      using (var session = isUpgradeRunning ? null : handlers.Domain.OpenSession(SessionType.KeyGenerator))
      using (var t = session == null ? null : session.OpenTransaction()) {
        var handler = (session ?? Session.Current).Handler;
        var queryExecutor = handler.GetService<IQueryExecutor>(true);
        if (!info.InsertRequest.IsNullOrEmpty())
          queryExecutor.ExecuteNonQuery(info.InsertRequest);
        object value = queryExecutor.ExecuteScalar(info.SelectRequest);
        hiValue = (TFieldType) Convert.ChangeType(value, typeof (TFieldType));
        // Intentionally rolling back the transaction!
      }

      var increment = generator.SequenceIncrement.Value;
      var domainHandler = (DomainHandler)handlers.DomainHandler;

      TFieldType current;
      var arithmetic = generator.Arithmetic;

      if (domainHandler.ProviderInfo.Supports(ProviderFeatures.Sequences) || domainHandler.ProviderInfo.Supports(ProviderFeatures.ArbitraryIdentityIncrement)) {
        // 256 - 1 * 128
        current = arithmetic.Subtract(hiValue, arithmetic.Multiply(arithmetic.One, increment));
      }
      else
        // (2 -1) * 128
        current = arithmetic.Multiply(arithmetic.Subtract(hiValue, arithmetic.One), increment);

      for (int i = 0; i < increment; i++) {
        current = arithmetic.Add(current, arithmetic.One);
        yield return current;
      }
    }

    private CachingKeyGeneratorInfo GetKeyGeneratorInfo<TFieldType>(CachingKeyGenerator<TFieldType> generator)
    {
      var domainHandler = (DomainHandler)handlers.DomainHandler;
      var sqlNext = GetNextImplementation(
        generator,
        domainHandler.ProviderInfo, 
        domainHandler.Schema, 
        generator.KeyInfo.MappingName);

      var batch = sqlNext as SqlBatch;
      if (batch != null && !domainHandler.ProviderInfo.Supports(ProviderFeatures.Batches)) {
        // No batches, so we must execute this manually
        return new CachingKeyGeneratorInfo {
          InsertRequest = domainHandler.Driver.Compile((ISqlCompileUnit) batch[0]).GetCommandText(),
          SelectRequest = domainHandler.Driver.Compile((ISqlCompileUnit) batch[1]).GetCommandText(),
        };
      }
      else
        // There are batches, so we can run this as a single request
        return new CachingKeyGeneratorInfo {
          SelectRequest = domainHandler.Driver.Compile(sqlNext).GetCommandText()
        };
    }

    /// <summary>
    /// Gets the "next sequence number" implementation.
    /// </summary>
    /// <param name="providerInfo">The provider info.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="sequenceMappingName">Name of the sequence mapping.</param>
    /// <returns>SQL compile unit making the necessary action.</returns>
    protected internal virtual ISqlCompileUnit GetCurrentValueImplementation(ProviderInfo providerInfo, Schema schema, string sequenceMappingName)
    {
      if (providerInfo.Supports(ProviderFeatures.Sequences))
        return GetSequenceBasedNextImplementation(null, providerInfo, schema, sequenceMappingName);
      else
        return GetTableBasedNextImplementation(null, providerInfo, schema, sequenceMappingName);
    }

    /// <summary>
    /// Gets the "next sequence number" implementation.
    /// </summary>
    /// <param name="providerInfo">The provider info.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="sequenceMappingName">Name of the sequence mapping.</param>
    /// <returns>SQL compile unit making the necessary action.</returns>
    protected internal virtual ISqlCompileUnit GetNextImplementation(KeyGenerator generator, ProviderInfo providerInfo, Schema schema, string sequenceMappingName)
    {
      if (providerInfo.Supports(ProviderFeatures.Sequences))
        return GetSequenceBasedNextImplementation(generator, providerInfo, schema, sequenceMappingName);
      else
        return GetTableBasedNextImplementation(generator, providerInfo, schema, sequenceMappingName);
    }

    /// <summary>
    /// Gets the "next sequence number" implementation based on real sequence.
    /// </summary>
    /// <param name="providerInfo">The provider info.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="sequenceMappingName">Name of the sequence mapping.</param>
    /// <returns>SQL compile unit making the necessary action.</returns>
    /// <exception cref="InvalidOperationException">Required sequence is not found.</exception>
    protected virtual ISqlCompileUnit GetSequenceBasedNextImplementation(KeyGenerator generator, ProviderInfo providerInfo, Schema schema, string sequenceMappingName)
    {
      var sequence = schema.Sequences
        .FirstOrDefault(s => s.Name==sequenceMappingName);
      if (sequence==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExSequenceXIsNotFoundInStorage, sequenceMappingName));
      if (generator != null)
        return SqlDml.Select(SqlDml.NextValue(sequence, (int) generator.SequenceIncrement.Value));
      return SqlDml.Select(SqlDml.NextValue(sequence));
    }

    /// <summary>
    /// Gets the "next sequence number" implementation based on regular table with
    /// auto-increment column.
    /// </summary>
    /// <param name="providerInfo">The provider info.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="sequenceMappingName">Name of the sequence mapping.</param>
    /// <returns>SQL compile unit making the necessary action.</returns>
    /// <exception cref="InvalidOperationException">Required schema object is not found.</exception>
    protected virtual ISqlCompileUnit GetTableBasedNextImplementation(KeyGenerator generator, ProviderInfo providerInfo, Schema schema, string sequenceMappingName)
    {
      var table = schema.Tables
        .FirstOrDefault(t => StringComparer.OrdinalIgnoreCase.Compare(t.Name, sequenceMappingName) == 0);
      if (table==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExTableXIsNotFound, sequenceMappingName));

      var idColumn = table.Columns
        .FirstOrDefault(c => c.Name==WellKnown.GeneratorColumnName) as TableColumn;
      if (idColumn==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExColumnXIsNotFoundInTableY, WellKnown.GeneratorColumnName, table.Name));

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);

      if (!providerInfo.Supports(ProviderFeatures.InsertDefaultValues)) {
        var fakeColumn = table.Columns
          .FirstOrDefault(c => c.Name==WellKnown.GeneratorFakeColumnName) as TableColumn;
        if (fakeColumn==null)
          throw new InvalidOperationException(
            string.Format(Strings.ExColumnXIsNotFoundInTableY, WellKnown.GeneratorFakeColumnName, table.Name));
        insert.Values[tableRef[fakeColumn.Name]] = SqlDml.Null;
      }

      var result = SqlDml.Batch();
      result.Add(insert);
      result.Add(SqlDml.Select(SqlDml.LastAutoGeneratedId()));
      return result;
    }


    // Constructors

    [ServiceConstructor]
    public CachingKeyGeneratorService(HandlerAccessor handlers)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      this.handlers = handlers;
    }
  }
}