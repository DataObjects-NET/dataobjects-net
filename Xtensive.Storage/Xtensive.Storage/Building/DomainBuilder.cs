// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Globalization;
using System.Reflection;
using Xtensive.Core;
using Xtensive.PluginManager;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Utility class for <see cref="Storage"/> building.
  /// </summary>
  public static class DomainBuilder
  {
    private static readonly PluginManager<ProviderAttribute> pluginManager =
      new PluginManager<ProviderAttribute>(typeof (HandlerFactory), Environment.CurrentDirectory);

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

      using (Log.InfoRegion(String.Format(Strings.LogValidatingX, typeof(DomainConfiguration).GetShortName())))
        Validate(configuration);

      var context = new BuildingContext(configuration);
      using (Log.InfoRegion(String.Format(Strings.LogBuildingX, typeof(Domain).GetShortName())))
      using (new BuildingScope(context)) {
        try {
          using (Log.InfoRegion(String.Format(Strings.LogCreatingX, typeof(Domain).GetShortName())))
            CreateDomain();
          using (Log.InfoRegion(String.Format(Strings.LogCreatingX, typeof(HandlerFactory).GetShortName())))
            CreateHandlerFactory();
          using (Log.InfoRegion(String.Format(Strings.LogCreatingX, typeof(NameBuilder).GetShortName())))
            CreateNameBuilder();
          using (Log.InfoRegion(String.Format(Strings.LogCreatingX, typeof(DomainHandler).GetShortName())))
            CreateDomainHandler();
          using (Log.InfoRegion(String.Format(Strings.LogBuildingX, Strings.Model)))
            BuildModel();
          using (Log.InfoRegion(String.Format(Strings.LogCreatingX, typeof(KeyManager).GetShortName())))
            CreateKeyManager();
          using (Log.InfoRegion(String.Format(Strings.LogCreatingX, Strings.Generators)))
            CreateGenerators();
        }
        catch (DomainBuilderException e) {
          context.RegisterError(e);
        }

        context.EnsureBuildSucceed();
      }
      return context.Domain;
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
      var domain = new Domain(BuildingContext.Current.Configuration);
      BuildingContext.Current.Domain = domain;
    }

    private static void CreateHandlerFactory()
    {
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
      var handlerFactory = (HandlerFactory) Activator.CreateInstance(handlerProviderType, new object[]{BuildingContext.Current.Domain});
      var handlerAccessor = BuildingContext.Current.Domain.HandlerAccessor;
      handlerAccessor.HandlerFactory = handlerFactory;
    }

    private static void CreateNameBuilder()
    {
      var handlerAccessor = BuildingContext.Current.Domain.HandlerAccessor;
      handlerAccessor.NameBuilder = handlerAccessor.HandlerFactory.CreateHandler<NameBuilder>();
    }

    private static void CreateDomainHandler()
    {
      var handlerAccessor = BuildingContext.Current.Domain.HandlerAccessor;
      handlerAccessor.DomainHandler = handlerAccessor.HandlerFactory.CreateHandler<DomainHandler>();
    }

    private static void BuildModel()
    {
      ModelBuilder.Build();
      var domain = BuildingContext.Current.Domain;
      domain.Model = BuildingContext.Current.Model;
    }

    private static void CreateKeyManager()
    {
      var domain = BuildingContext.Current.Domain;
      var handlerAccessor = BuildingContext.Current.Domain.HandlerAccessor;
      handlerAccessor.KeyManager = new KeyManager(domain);
    }

    private static void CreateGenerators()
    {
      var handlerAccessor = BuildingContext.Current.Domain.HandlerAccessor;
      Registry<HierarchyInfo, DefaultGenerator> generators = BuildingContext.Current.Domain.KeyManager.Generators;
      foreach (HierarchyInfo hierarchy in BuildingContext.Current.Model.Hierarchies) {
        DefaultGenerator generator;
        if (hierarchy.Generator==typeof (DefaultGenerator))
          generator = handlerAccessor.HandlerFactory.CreateHandler<DefaultGenerator>();
        else
          generator = (DefaultGenerator) Activator.CreateInstance(hierarchy.Generator);
        generator.Hierarchy = hierarchy;
        generator.Initialize();
        generators.Register(hierarchy, generator);
      }
      generators.Lock();
    }
  }
}
