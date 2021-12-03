// Copyright (C) 2018-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2018.10.01

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Reflection;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public abstract class ModelBuildingTest
  {
    private const string ErrorInTestFixtureSetup = "Error in TestFixtureSetUp:\r\n{0}";

    protected bool shouldBuildRealDomain = false;

    [OneTimeSetUp]
    public void TestFixureSetUp()
    {
      try {
        CheckRequirements();
        PopulateData();
      }
      catch (IgnoreException) {
        throw;
      }
      catch (Exception e) {
        Debug.WriteLine(ErrorInTestFixtureSetup, e);
        throw;
      }
    }

    protected virtual void CheckRequirements()
    {
    }

    protected virtual void PopulateData()
    {
    }

    protected void TestInvoker(DomainConfiguration domainConfiguration, Action<Domain> domainValidationAction)
      => TestInvoker(domainConfiguration, domainValidationAction, null);

    protected void TestInvoker(DomainConfiguration domainConfiguration, Action<DomainModelDef> domainDefValidationAction)
      => TestInvoker(domainConfiguration, null, domainDefValidationAction);

    protected void TestInvoker(DomainConfiguration domainConfiguration, Action<Domain> domainValidationAction, Action<DomainModelDef> domainDefsValidationAction)
    {
      domainConfiguration.Types.Register(typeof(ModelDefCapturer));

      var upgradeContext = new UpgradeContext(domainConfiguration);

      if (shouldBuildRealDomain) {
        using (var domain = Domain.Build(domainConfiguration)) {
          var modelDef = domain.Extensions.Get<DomainModelDef>();
          if (domainDefsValidationAction != null && modelDef != null) {
            domainDefsValidationAction.Invoke(modelDef);
          }

          if (domainValidationAction != null) {
            domainValidationAction.Invoke(domain);
          }
        }
      }
      else {
        using (upgradeContext.Activate())
        using (upgradeContext.Services) {
          BuildServices(upgradeContext);
          var domain = CreateDomainBuilder(upgradeContext).Invoke();
          var modelDef = domain.Extensions.Get<DomainModelDef>();
          if (domainDefsValidationAction != null && modelDef != null)
            domainDefsValidationAction.Invoke(modelDef);
          if (domainValidationAction != null)
            domainValidationAction.Invoke(domain);
        }
      }
    }

    private Func<Domain> CreateDomainBuilder(UpgradeContext context)
    {
      var buildingConfiguration = BuildBuilderConfiguration(context);

      Func<DomainBuilderConfiguration, Domain> builder = DomainBuilder.Run;
      return builder.Bind(buildingConfiguration);
    }

    private DomainBuilderConfiguration BuildBuilderConfiguration(UpgradeContext context)
    {
      var configuration = new DomainBuilderConfiguration {
        DomainConfiguration = context.Configuration,
        Stage = context.Stage,
        Services = context.Services,
        ModelFilter = new StageModelFilter(context.UpgradeHandlers, UpgradeStage.Final),
        UpgradeContextCookie = context.Cookie,
        RecycledDefinitions = context.RecycledDefinitions,
        DefaultSchemaInfo = null
      };

      return configuration;
    }

    private void BuildServices(UpgradeContext context)
    {
      var services = context.Services;
      var configuration = context.Configuration;

      services.Configuration = configuration;
      services.IndexFilterCompiler = new PartialIndexFilterCompiler();

      var descriptor = ProviderDescriptor.Get(configuration.ConnectionInfo.Provider);
      var driverFactory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);
      var handlerFactory = (HandlerFactory) Activator.CreateInstance(descriptor.HandlerFactory);
      var driver = StorageDriver.Create(driverFactory, configuration);
      services.HandlerFactory = handlerFactory;
      services.StorageDriver = driver;
      services.NameBuilder = new NameBuilder(configuration, driver.ProviderInfo);
      BuildExternalServices(services, context);

      services.Lock();

      context.TypeIdProvider = new TypeIdProvider(context);
    }

    private void BuildExternalServices(UpgradeServiceAccessor serviceAccessor, UpgradeContext context)
    {
      var configuration = context.Configuration;
      var standardRegistrations = new[] {
        new ServiceRegistration(typeof(DomainConfiguration), context.Configuration),
        new ServiceRegistration(typeof(UpgradeContext), context)
      };

      var modules = configuration.Types.Modules
        .Select(type => new ServiceRegistration(typeof(IModule), type, false));
      var handlers = configuration.Types.UpgradeHandlers
        .Select(type => new ServiceRegistration(typeof(IUpgradeHandler), type, false));

      var registrations = standardRegistrations.Concat(modules).Concat(handlers);
      var serviceContainer = new ServiceContainer(registrations);
      serviceAccessor.RegisterResource(serviceContainer);

      BuildModules(serviceAccessor, serviceContainer);
      BuildUpgradeHandlers(serviceAccessor, serviceContainer);
    }

    private static void BuildModules(UpgradeServiceAccessor serviceAccessor, IServiceContainer serviceContainer)
      => serviceAccessor.Modules = serviceContainer.GetAll<IModule>().ToList().AsReadOnly();

    private static void BuildUpgradeHandlers(UpgradeServiceAccessor serviceAccessor, IServiceContainer serviceContainer)
    {
      // Getting user handlers
      var userHandlers =
        from handler in serviceContainer.GetAll<IUpgradeHandler>()
        let assembly = handler.Assembly ?? handler.GetType().Assembly
        where handler.IsEnabled
        group handler by assembly;

      // Adding user handlers
      var handlers = new Dictionary<Assembly, IUpgradeHandler>();
      foreach (var group in userHandlers) {
        var candidates = group.ToList();
        if (candidates.Count > 1) {
          throw new DomainBuilderException(
            string.Format(Strings.ExMoreThanOneEnabledXIsProvidedForAssemblyY, typeof(IUpgradeHandler).GetShortName(), @group.Key));
        }
        handlers.Add(group.Key, candidates[0]);
      }

      // Adding default handlers
      var assembliesWithUserHandlers = handlers.Select(pair => pair.Key);
      var assembliesWithoutUserHandler =
        serviceAccessor.Configuration.Types.PersistentTypes
          .Select(type => type.Assembly)
          .Distinct()
          .Except(assembliesWithUserHandlers);

      foreach (var assembly in assembliesWithoutUserHandler) {
        var handler = new UpgradeHandler(assembly);
        handlers.Add(assembly, handler);
      }

      // Building a list of handlers sorted by dependencies of their assemblies
      var dependencies = handlers.Keys.ToDictionary(
        assembly => assembly,
        assembly => new HashSet<string>(assembly.GetReferencedAssemblies().Select(assemblyName => assemblyName.ToString())));
      var sortedHandlers = handlers
        .SortTopologically((a0, a1) => dependencies[a1.Key].Contains(a0.Key.GetName().ToString()))
        .Select(pair => pair.Value);

      // Storing the result
      serviceAccessor.UpgradeHandlers =
        new ReadOnlyDictionary<Assembly, IUpgradeHandler>(handlers);
      serviceAccessor.OrderedUpgradeHandlers =
        sortedHandlers.ToList().AsReadOnly();
    }

    public sealed class ModelDefCapturer : IModule
    {
      public void OnBuilt(Domain domain)
      {
      }

      public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
      {
        var domain = context.Domain;
        domain.Extensions.Set(model);
      }
    }
  }
}