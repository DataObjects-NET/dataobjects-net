// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;

namespace Xtensive.Orm.Building.Builders
{
  /// <summary>
  /// Utility class for <see cref="Domain"/> building.
  /// </summary>
  internal sealed class DomainBuilder
  {
    private readonly BuildingContext context;

    /// <summary>
    /// Builds the domain.
    /// </summary>
    /// <param name="builderConfiguration">The builder configuration.</param>
    /// <returns>Built domain.</returns>
    public static Domain Run(DomainBuilderConfiguration builderConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(builderConfiguration, "builderConfiguration");

      var context = new BuildingContext(builderConfiguration);
      using (BuildLog.InfoRegion(Strings.LogBuildingX, typeof (Domain).GetShortName()))
        new DomainBuilder(context).Run();

      return context.Domain;
    }

    private void Run()
    {
      CreateDomain();
      CreateHandlers();
      BuildModel();
      CreateServices();
      InitializeServices();
    }

    private void CreateDomain()
    {
      using (BuildLog.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        var services = context.BuilderConfiguration.Services;
        var sharedConnection =
          services.ProviderInfo.Supports(ProviderFeatures.SharedConnection)
            ? services.Connection
            : null;
        context.Domain = new Domain(
          context.Configuration,
          context.BuilderConfiguration.UpgradeContextCookie,
          sharedConnection);
      }
    }

    private void CreateHandlers()
    {
      var configuration = context.Domain.Configuration;
      var handlers = context.Domain.Handlers;
      var services = context.BuilderConfiguration.Services;

      using (BuildLog.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        // HandlerFactory
        handlers.Factory = services.HandlerFactory;

        // NameBuilder
        handlers.NameBuilder = services.NameBuilder;

        // SchemaResolver
        handlers.MappingResolver = services.Resolver;

        // StorageDriver
        handlers.StorageDriver = services.Driver.CreateNew(context.Domain);

        // GeneratorQueryBuilder
        handlers.SequenceQueryBuilder = new SequenceQueryBuilder(handlers.StorageDriver);

        // DomainHandler
        handlers.DomainHandler = handlers.Create<DomainHandler>();
        handlers.DomainHandler.BuildHandlers();
      }
    }

    private void CreateServices()
    {
      using (BuildLog.InfoRegion(Strings.LogCreatingX, typeof (IServiceContainer).GetShortName())) {
        var domain = context.Domain;
        var configuration = domain.Configuration;
        var userContainerType = configuration.ServiceContainerType ?? typeof (ServiceContainer);
        var registrations = CreateServiceRegistrations(configuration).Concat(KeyGeneratorFactory.GetRegistrations(context));
        var systemContainer = domain.CreateSystemServices();
        var userContainer = ServiceContainer.Create(userContainerType, systemContainer);
        domain.Services = new ServiceContainer(registrations, userContainer);
      }
    }

    private void BuildModel()
    {
      using (BuildLog.InfoRegion(Strings.LogBuildingX, Strings.Model)) {
        ModelBuilder.Run(context);
        var model = context.Model;
        model.Lock(true);
        context.Domain.Model = model;
      }
    }

    private void InitializeServices()
    {
      using (BuildLog.InfoRegion(Strings.LogBuildingX, Strings.KeyGenerators)) {
        var domain = context.Domain;
        var generators = domain.KeyGenerators;
        var initialized = new HashSet<KeyGenerator>();
        var keysToProcess = domain.Model.Hierarchies
          .Select(h => h.Key)
          .Where(k => k.GeneratorKind!=KeyGeneratorKind.None);
        foreach (var keyInfo in keysToProcess) {
          var generator = domain.Services.Demand<KeyGenerator>(keyInfo.GeneratorName);
          generators.Register(keyInfo, generator);
          if (keyInfo.IsFirstAmongSimilarKeys)
            generator.Initialize(context.Domain, keyInfo.TupleDescriptor);
          var temporaryGenerator = domain.Services.Get<TemporaryKeyGenerator>(keyInfo.GeneratorName);
          if (temporaryGenerator==null)
            continue; // Temporary key generators are optional
          generators.RegisterTemporary(keyInfo, temporaryGenerator);
          if (keyInfo.IsFirstAmongSimilarKeys)
            temporaryGenerator.Initialize(context.Domain, keyInfo.TupleDescriptor);
        }
        generators.Lock();
      }

      // Initialize DomainHandler services as well
      context.Domain.Handler.InitializeServices();
    }

    private static IEnumerable<ServiceRegistration> CreateServiceRegistrations(DomainConfiguration configuration)
    {
      return configuration.Types.DomainServices.SelectMany(ServiceRegistration.CreateAll);
    }

    // Constructors

    private DomainBuilder(BuildingContext context)
    {
      this.context = context;
    }
  }
}
