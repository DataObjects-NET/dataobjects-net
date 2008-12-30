// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Globalization;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.PluginManager;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  /// <summary>
  /// Utility class for <see cref="Storage"/> building.
  /// </summary>
  public static class DomainBuilder
  {
    private static readonly PluginManager<ProviderAttribute> pluginManager =
      new PluginManager<ProviderAttribute>(typeof (HandlerFactory), AppDomain.CurrentDomain.BaseDirectory);

    /// <summary>
    /// Builds the new <see cref="Domain"/> according to specified configuration.
    /// </summary>
    /// <param name="configuration">The storage configuration.</param>
    /// <returns>Newly created <see cref="Domain"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="configuration"/> is null.</exception>
    /// <exception cref="DomainBuilderException">When at least one error have been occurred 
    /// during storage building process.</exception>
    public static Domain Build(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      if (!configuration.IsLocked)
        configuration.Lock(true);

      using (LogTemplate<Log>.InfoRegion(Strings.LogValidatingX, typeof (DomainConfiguration).GetShortName()))
        Validate(configuration);

      var context = new BuildingContext(configuration);

      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, typeof (Domain).GetShortName())) {
        using (new BuildingScope(context)) {
          try {
            CreateDomain();
            CreateHandlerFactory();
            CreateNameBuilder();
            BuildModel();
            CreateDomainHandler();
            using (context.Domain.Handler.OpenSession(SessionType.System)) {
              using (var transactionScope = Transaction.Open()) {
                BuildingScope.Context.SystemSessionHandler = Session.Current.Handler;
                using (LogTemplate<Log>.InfoRegion(String.Format(Strings.LogBuildingX, typeof (DomainHandler).GetShortName()))) {
                  bool sysTables = context.Domain.Handler.CheckSystemTypes();
                  if (!sysTables) {
                    context.Domain.Handler.BuildRecreate();
                    CreateGenerators();
                  }
                  else if (!CheckAssemblyVersions())
                    using (new UpgradeScope(new UpgradeContext())) {
                      BuildRcModel();
                      context.Domain.Handler.BuildRecycling(); // Creating rcTables, copy data, create new structures
                      CreateGenerators();
                      RunUpgradeScripts();
                      context.Domain.Handler.DeleteRecycledData();
                      UpdateAssembliesData();
                    }
                  else {
                    switch (context.Domain.Configuration.BuildMode) {
                    case DomainBuildMode.Perform:
                      context.Domain.Handler.BuildPerform();
                      break;
                    case DomainBuildMode.Skip:
                      throw new NotImplementedException();
                      break;
                    case DomainBuildMode.SkipButExtract:
                      throw new NotImplementedException();
                      break;
                    case DomainBuildMode.SkipButExtractAndCompare:
                      throw new NotImplementedException();
                      break;
                    case DomainBuildMode.PerformSafe:
                      throw new NotImplementedException();
                      break;
                    case DomainBuildMode.PerformComplete:
                      throw new NotImplementedException();
                      break;
                    case DomainBuildMode.Recreate:
                      context.Domain.Handler.BuildRecreate();
                      break;
                    case DomainBuildMode.Block:
                        throw new NotImplementedException();
                      break;
                    default:
                        throw new NotImplementedException();
                    }
                    CreateGenerators();
                  }
                }
                transactionScope.Complete();
              }
            }
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }

          context.EnsureBuildSucceed();
        }
      }
      return context.Domain;
    }

    private static void UpdateAssembliesData()
    {
      throw new NotImplementedException();
    }

    private static void RunUpgradeScripts()
    {
      throw new NotImplementedException();
    }

    private static void BuildRcModel()
    {
      throw new NotImplementedException();
    }

    private static bool CheckAssemblyVersions()
    {
      // TODO: Implement
      return true;
      throw new NotImplementedException();
    }

    #region ValidateXxx methods

    private static void Validate(DomainConfiguration configuration)
    {
      if (configuration.Builders.Count > 0)
        foreach (Type type in configuration.Builders)
          ValidateBuilder(type);
    }

    private static void ValidateBuilder(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      ConstructorInfo constructor =
        type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);

      if (constructor==null)
        throw new DomainBuilderException(
          string.Format(Strings.ExTypeXMustHavePublicInstanceParameterlessConstructorInOrderToBeUsedAsStorageDefinitionBuilder, type.FullName));

      if (!typeof (IDomainBuilder).IsAssignableFrom(type))
        throw new DomainBuilderException(
          string.Format(CultureInfo.CurrentCulture,
            Strings.ExTypeXDoesNotImplementYInterface, type.FullName, typeof (IDomainBuilder).FullName));
    }

    #endregion

    private static void CreateDomain()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        var domain = new Domain(BuildingContext.Current.Configuration);
        BuildingContext.Current.Domain = domain;
      }
    }

    private static void CreateHandlerFactory()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, typeof (HandlerFactory).GetShortName())) {
        string protocol = BuildingContext.Current.Configuration.ConnectionInfo.Protocol;
        Type handlerProviderType;
        lock (pluginManager) {
          handlerProviderType = pluginManager[new ProviderAttribute(protocol)];
          if (handlerProviderType==null)
            throw new DomainBuilderException(
              string.Format(Strings.ExStorageProviderNotFound,
                protocol,
                Environment.CurrentDirectory));
        }
        var handlerFactory = (HandlerFactory) Activator.CreateInstance(handlerProviderType, new object[] {BuildingContext.Current.Domain});
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        handlerAccessor.HandlerFactory = handlerFactory;
      }
    }

    private static void CreateNameBuilder()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, typeof (NameBuilder).GetShortName())) {
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        handlerAccessor.NameBuilder = handlerAccessor.HandlerFactory.CreateHandler<NameBuilder>();
        handlerAccessor.NameBuilder.Initialize(handlerAccessor.Domain.Configuration.NamingConvention);
      }
    }

    private static void CreateDomainHandler()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        handlerAccessor.DomainHandler = handlerAccessor.HandlerFactory.CreateHandler<DomainHandler>();
        handlerAccessor.DomainHandler.Initialize();
      }
    }

    private static void BuildModel()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Model)) {
        ModelBuilder.Build();
        var domain = BuildingContext.Current.Domain;
        domain.Model = BuildingContext.Current.Model;
      }
    }

    private static void CreateGenerators()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, Strings.Generators)) {
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        var keyGenerators = BuildingContext.Current.Domain.KeyGenerators;
        var generatorFactory = handlerAccessor.HandlerFactory.CreateHandler<KeyGeneratorFactory>();
        foreach (HierarchyInfo hierarchy in BuildingContext.Current.Model.Hierarchies) {
          KeyGenerator keyGenerator;
          if (hierarchy.KeyGeneratorType==null)
            continue;
          if (hierarchy.KeyGeneratorType==typeof (KeyGenerator))
            keyGenerator = generatorFactory.CreateGenerator(hierarchy);
          else
            keyGenerator = (KeyGenerator) Activator.CreateInstance(hierarchy.KeyGeneratorType, new object[] {hierarchy});
          keyGenerator.Initialize();
          keyGenerators.Register(hierarchy, keyGenerator);
        }
        keyGenerators.Lock();
      }
    }
  }
}