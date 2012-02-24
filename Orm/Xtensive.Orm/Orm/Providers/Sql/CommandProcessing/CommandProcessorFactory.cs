// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.18

using Xtensive.Orm;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A handlers that is capable of creating <see cref="CommandProcessor"/>s.
  /// </summary>
  public class CommandProcessorFactory : InitializableHandlerBase
  {
    private DomainHandler domainHandler;

    /// <summary>
    /// Creates the command processor.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="connection">The connection.</param>
    /// <returns>Created command processor.</returns>
    public CommandProcessor CreateCommandProcessor(Session session, SqlConnection connection)
    {
      int batchSize = session.Configuration.BatchSize;
      bool useBatches = batchSize > 1
        && domainHandler.ProviderInfo.Supports(ProviderFeatures.DmlBatches);
      bool useCursorParameters =
        domainHandler.ProviderInfo.Supports(ProviderFeatures.MultipleResultsViaCursorParameters);

      var factory = useCursorParameters
        ? new CursorCommandPartFactory(domainHandler.Driver, connection)
        : new CommandPartFactory(domainHandler.Driver, connection);

      var processor = useBatches
        ? new BatchingCommandProcessor(session, factory, batchSize)
        : (CommandProcessor) new SimpleCommandProcessor(session, factory);
      return processor;
      
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      domainHandler = (DomainHandler) Handlers.DomainHandler;
    }
  }
}