// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
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
      ArgumentValidator.EnsureArgumentNotNull(builderConfiguration, nameof(builderConfiguration));

      var context = new BuildingContext(builderConfiguration);
      using (BuildLog.InfoRegion(nameof(Strings.LogBuildingX), typeof(Domain).GetShortName())) {
        new DomainBuilder(context).Run();
      }

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
      using (BuildLog.InfoRegion(nameof(Strings.LogCreatingX), typeof(Domain).GetShortName())) {
        var services = context.BuilderConfiguration.Services;
        var useSingleConnection =
          services.ProviderInfo.Supports(ProviderFeatures.SingleConnection)
          && context.BuilderConfiguration.Stage == UpgradeStage.Final;

        context.Domain = new Domain(
          context.Configuration,
          context.BuilderConfiguration.UpgradeContextCookie,
          useSingleConnection ? services.Connection : null);
      }
    }

    private void CreateHandlers()
    {
      var handlers = context.Domain.Handlers;
      var services = context.BuilderConfiguration.Services;

      using (BuildLog.InfoRegion(nameof(Strings.LogCreatingX), typeof(DomainHandler).GetShortName())) {
        // HandlerFactory
        handlers.Factory = services.HandlerFactory;

        // NameBuilder
        handlers.NameBuilder = services.NameBuilder;

        // StorageDriver
        handlers.StorageDriver = services.StorageDriver.CreateNew(context.Domain);

        // SequenceQueryBuilder
        handlers.SequenceQueryBuilder = new SequenceQueryBuilder(handlers.StorageDriver);

        // StorageNodeRegistry
        handlers.StorageNodeRegistry = new StorageNodeRegistry();

        // DomainHandler
        handlers.DomainHandler = handlers.Create<DomainHandler>();
        handlers.DomainHandler.BuildHandlers();
      }
    }

    private void CreateServices()
    {
      using (BuildLog.InfoRegion(nameof(Strings.LogCreatingX), typeof(IServiceContainer).GetShortName())) {
        var domain = context.Domain;
        var configuration = domain.Configuration;
        var userContainerType = configuration.ServiceContainerType ?? typeof(ServiceContainer);
        var registrations = CreateServiceRegistrations(configuration)
          .Concat(KeyGeneratorFactory.GetRegistrations(context));
        var systemContainer = domain.CreateSystemServices();
        var userContainer = ServiceContainer.Create(userContainerType, systemContainer);
        domain.Services = new ServiceContainer(registrations, userContainer);
      }
    }

    private void BuildModel()
    {
      using (BuildLog.InfoRegion(nameof(Strings.LogBuildingX), Strings.Model)) {
        ModelBuilder.Run(context);
        var model = context.Model;
        model.Lock(true);
        context.Domain.Model = model;
      }
    }

    private void InitializeServices()
    {
      var domain = context.Domain;

      using (BuildLog.InfoRegion(nameof(Strings.LogBuildingX), Strings.KeyGenerators)) {
        var generators = domain.KeyGenerators;
        var keysToProcess = domain.Model.Hierarchies
          .Select(h => h.Key)
          .Where(k => k.GeneratorKind != KeyGeneratorKind.None);
        foreach (var keyInfo in keysToProcess) {
          var generator = domain.Services.Demand<KeyGenerator>(keyInfo.GeneratorName);
          generators.Register(keyInfo, generator);
          if (keyInfo.IsFirstAmongSimilarKeys) {
            generator.Initialize(context.Domain, keyInfo.TupleDescriptor);
          }

          var temporaryGenerator = domain.Services.Get<TemporaryKeyGenerator>(keyInfo.GeneratorName);
          if (temporaryGenerator == null) {
            continue; // Temporary key generators are optional
          }

          generators.RegisterTemporary(keyInfo, temporaryGenerator);
          if (keyInfo.IsFirstAmongSimilarKeys) {
            temporaryGenerator.Initialize(context.Domain, keyInfo.TupleDescriptor);
          }
        }

        generators.Lock();
      }

      using (BuildLog.InfoRegion(nameof(Strings.LogBuildingX), Strings.Validators)) {
        foreach (var type in domain.Model.Types) {
          foreach (var validator in type.Validators) {
            validator.Configure(domain, type);
          }

          foreach (var field in type.Fields) {
            foreach (var validator in field.Validators) {
              validator.Configure(domain, type, field);
            }
          }
        }
      }

      // Initialize DomainHandler services as well
      domain.Handler.InitializeServices();
    }

    private static IEnumerable<ServiceRegistration> CreateServiceRegistrations(DomainConfiguration configuration) =>
      configuration.Types.DomainServices.SelectMany(ServiceRegistration.CreateAll);

    // Constructors

    private DomainBuilder(BuildingContext context)
    {
      this.context = context;
    }
  }
}
