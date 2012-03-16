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

      using (Log.InfoRegion(Strings.LogBuildingX, typeof (Domain).GetShortName()))
      using (new BuildingScope(context))
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
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        context.Domain = new Domain(context.Configuration);
      }
    }

    private void CreateHandlers()
    {
      var configuration = context.Domain.Configuration;
      var handlers = context.Domain.Handlers;
      var services = context.BuilderConfiguration.Services;

      using (Log.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        // HandlerFactory
        handlers.Factory = services.HandlerFactory;

        // NameBuilder
        handlers.NameBuilder = services.NameBuilder;

        // SchemaResolver
        handlers.SchemaNodeResolver = services.Resolver;

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
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (IServiceContainer).GetShortName())) {
        var domain = context.Domain;
        var configuration = domain.Configuration;
        var serviceContainerType = configuration.ServiceContainerType ?? typeof (ServiceContainer);
        var registrations = CreateServiceRegistrations(configuration)
          .Concat(KeyGeneratorFactory.GetRegistrations(context));
        var baseServiceContainer = domain.Handler.CreateBaseServices();
        domain.Services = ServiceContainer.Create(
          typeof (ServiceContainer), registrations, ServiceContainer.Create(serviceContainerType, baseServiceContainer));
      }
    }

    private void BuildModel()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Model)) {
        ModelBuilder.Run(context);
        var model = context.Model;
        model.Lock(true);
        context.Domain.Model = model;
      }
    }

    private void InitializeServices()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.KeyGenerators)) {
        var domain = context.Domain;
        var generators = domain.KeyGenerators;
        var initialized = new HashSet<IKeyGenerator>();
        var keysToProcess = domain.Model.Hierarchies
          .Select(h => h.Key)
          .Where(k => k.GeneratorKind!=KeyGeneratorKind.None);
        foreach (var keyInfo in keysToProcess) {
          var generator = domain.Services.Demand<IKeyGenerator>(keyInfo.GeneratorName);
          generators.Register(keyInfo, generator);
          if (keyInfo.IsFirstAmongSimilarKeys)
            generator.Initialize(context.Domain, keyInfo.TupleDescriptor);
          var temporaryGenerator = domain.Services.Get<ITemporaryKeyGenerator>(keyInfo.GeneratorName);
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
