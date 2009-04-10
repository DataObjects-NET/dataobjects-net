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
using Xtensive.Core.Tuples;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.PluginManager;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Activator=System.Activator;
using TypeInfo=Xtensive.Storage.Model.TypeInfo;


namespace Xtensive.Storage.Building.Builders
{
  /// <summary>
  /// Utility class for <see cref="Storage"/> building.
  /// </summary>
  public static class DomainBuilder
  {
    private static readonly PluginManager<ProviderAttribute> pluginManager =
      new PluginManager<ProviderAttribute>(typeof (HandlerFactory), AppDomain.CurrentDomain.BaseDirectory);

    internal static void BuildForUpgrade(DomainConfiguration configuration, Action upgradeDataProcessor)
    {
      InternalBuild(configuration, 
        UpdateSchema,
        upgradeDataProcessor);
    }

    internal static void BuildForAccessMetadata(DomainConfiguration configuration, Action metadataReader)
    {
      InternalBuild(configuration, 
        () => { },
        metadataReader);
    }

    private static Domain BuildBlockUpgrade(DomainConfiguration configuration)
    {
      SchemaUpgradeHelper helper = new SchemaUpgradeHelper(configuration);
      return InternalBuild(configuration, 
        () => { },
        helper.CheckSchemaIsActual);
    }

    private static Domain BuildRecreate(DomainConfiguration configuration)
    {
      SchemaUpgradeHelper helper = new SchemaUpgradeHelper(configuration);
      return InternalBuild(configuration, 
        RecreateSchema, 
        helper.SetInitialSchemaVersion);
    }

    private static Domain BuildPerformStrict(DomainConfiguration configuration)
    {
      SchemaUpgradeHelper helper = new SchemaUpgradeHelper(configuration);
      helper.UpgradeData();
      return BuildBlockUpgrade(configuration);
    }

    private static void RecreateSchema()
    {
      var context = BuildingContext.Current;
      var upgradeHandler = context.HandlerFactory.CreateHandler<SchemaUpgradeHandler>();
      upgradeHandler.ClearStorageSchema();
      upgradeHandler.UpdateStorageSchema();
    }

    private static void UpdateSchema()
    {
      var context = BuildingContext.Current;
      var upgradeHandler = context.HandlerFactory.CreateHandler<SchemaUpgradeHandler>();
      upgradeHandler.UpdateStorageSchema();
    }

    public static Domain InternalBuild(DomainConfiguration configuration, Action schemaProcessor, Action dataProcessor)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      if (!configuration.IsLocked)
        configuration.Lock(true);
      
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
                var sessionHandler = Session.Current.Handler;
                BuildingScope.Context.SystemSessionHandler = sessionHandler;
                BuildingContext.Current.Domain.Handler.InitializeSystemSession();
                // CreateGenerators();

                schemaProcessor.Invoke();

                context.Domain.Handler.BuildMappingSchema();
                CreateGenerators();

                dataProcessor.Invoke();

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
      switch (configuration.BuildMode) {
        case DomainBuildMode.Recreate: 
          return BuildRecreate(configuration);
        case DomainBuildMode.BlockUpgrade:
          return BuildBlockUpgrade(configuration);
        case DomainBuildMode.PerformStrict:
          return BuildPerformStrict(configuration);
        

        default:
          return BuildRecreate(configuration);
      }
      
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

    private static void AssignSystemTypeIds()
    {
      foreach (TypeInfo type in BuildingContext.Current.Model.Types)
        if (type.IsSystem)
          type.TypeId = BuildingContext.Current.SystemTypeIds[type.UnderlyingType];
    }

    private static void ReadExistingTypeIds()
    {
      //TODO: Optimize this code
      foreach (Metadata.Type type in Query<Metadata.Type>.All) {
        foreach (TypeInfo typeInfo in BuildingContext.Current.Model.Types) {
          if (typeInfo.UnderlyingType.FullName==type.FullName)
            typeInfo.TypeId = type.Id;
        }
      }
    }

    private static void GenerateNewTypeIds()
    {
    }


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
        foreach (var generatorInfo in BuildingContext.Current.Model.Generators) {
          KeyGenerator keyGenerator;
          if (generatorInfo.Type==null)
            continue;
          if (generatorInfo.Type==typeof (KeyGenerator))
            keyGenerator = generatorFactory.CreateGenerator(generatorInfo);
          else
            keyGenerator = (KeyGenerator) Activator.CreateInstance(generatorInfo.Type, new object[] {generatorInfo});
          keyGenerator.Initialize();
          keyGenerators.Register(generatorInfo, keyGenerator);
        }
        keyGenerators.Lock();
      }
    }

  }
}
