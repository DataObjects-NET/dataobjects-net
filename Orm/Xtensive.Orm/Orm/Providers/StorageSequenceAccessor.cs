// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Standard <see cref="IStorageSequenceAccessor"/> implementation.
  /// </summary>
  [Service(typeof (IStorageSequenceAccessor))]
  public class StorageSequenceAccessor : IStorageSequenceAccessor
  {
    private readonly Domain domain;
    private readonly SequenceQueryBuilder queryBuilder;
    private readonly bool hasArbitaryIncrement;
    private readonly bool hasSequences;

    /// <inheritdoc/>
    public Segment<long> NextBulk(SequenceInfo sequenceInfo, Session session)
    {
      var generatorNode = GetGeneratorNode(sequenceInfo, session.StorageNode);
      var query = queryBuilder.BuildNextValueQuery(generatorNode, sequenceInfo.Increment);

      long hiValue = Execute(query, session);

      var increment = sequenceInfo.Increment;
      var current = hasArbitaryIncrement ? hiValue - increment : (hiValue - 1) * increment;

      return new Segment<long>(current + 1, increment);
    }


    /// <inheritdoc/>
    public void CleanUp(IEnumerable<SequenceInfo> sequences, Session session)
    {
      if (hasSequences)
        return;

      var statements = sequences.Select(s => queryBuilder.BuildCleanUpQuery(GetGeneratorNode(s, session.StorageNode)));
      session.Services.Demand<ISqlExecutor>().ExecuteMany(statements);
    }

    private long Execute(SequenceQuery query, Session session)
    {
      var compartment = UpgradeContext.GetCurrent(session.Domain.UpgradeContextCookie)!=null
        ? SequenceQueryCompartment.SameSession
        : query.Compartment;

      switch (compartment) {
        case SequenceQueryCompartment.SameSession:
          return query.ExecuteWith(session.Services.Demand<ISqlExecutor>());
        case SequenceQueryCompartment.SeparateSession:
          return ExecuteInKeyGeneratorSession(query, session.StorageNode);
        default:
          throw new ArgumentOutOfRangeException("query.Compartment");
      }
    }

    private long ExecuteInKeyGeneratorSession(SequenceQuery query, StorageNode node)
    {
      long result;
      using (var session = domain.OpenSession(SessionType.KeyGenerator)) {
        session.SetStorageNode(node);
        using (session.OpenTransaction()) {
          result = query.ExecuteWith(session.Services.Demand<ISqlExecutor>());
          // Rollback
        }
      }
      return result;
    }

    private SchemaNode GetGeneratorNode(SequenceInfo sequenceInfo, StorageNode storageNode)
    {
      var result = storageNode.Mapping[sequenceInfo];
      if (result!=null)
        return result;

      var message = string.Format(
        hasSequences ? Strings.ExSequenceXIsNotFoundInStorage : Strings.ExTableXIsNotFound,
        sequenceInfo.MappingName);
      throw new InvalidOperationException(message);
    }

    // Constructors

    [ServiceConstructor]
    public StorageSequenceAccessor(HandlerAccessor handlers)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");

      queryBuilder = handlers.SequenceQueryBuilder;
      domain = handlers.Domain;

      var providerInfo = handlers.ProviderInfo;
      hasArbitaryIncrement =
        providerInfo.Supports(ProviderFeatures.Sequences)
        || providerInfo.Supports(ProviderFeatures.ArbitraryIdentityIncrement);
      hasSequences = providerInfo.Supports(ProviderFeatures.Sequences);
    }
  }
}