// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.PluginManager;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using TypeInfo=Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Utility class for <see cref="Storage"/> building.
  /// </summary>
  public static class DomainBuilder
  {
    private static readonly PluginManager<HandlerProviderAttribute> pluginManager =
      new PluginManager<HandlerProviderAttribute>(typeof (HandlerProvider), Environment.CurrentDirectory);

    /// <summary>
    /// Builds the new <see cref="Storage"/> according to specified configuration.
    /// </summary>
    /// <param name="configuration">The storage configuration.</param>
    /// <returns>Newly created <see cref="Storage"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="configuration"/> is null.</exception>
    /// <exception cref="DomainBuilderException">When at least one error have been occurred 
    /// during storage building process.</exception>
    public static Domain Build(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      if (!configuration.IsLocked)
        configuration.Lock(true);

      Validate(configuration);

      BuildingContext buildingContext = new BuildingContext(configuration);
      buildingContext.NameProvider = new NameProvider(configuration.NamingConvention);

      using (new BuildingScope(buildingContext)) {
        using (var scope = new LogCaptureScope(BuildingScope.Context.Logger)) {
          BuildModel();
          BuildStorage();
          BuildHandlerProvider();
          BuildKeyProviders();
          if (scope.IsCaptured(LogEventTypes.Error))
            throw new DomainBuilderException(Strings.ExErrorsDuringStorageBuild);
        }
      }
      return buildingContext.Domain;
    }

    private static void BuildModel()
    {
      ModelBuilder.Build();
    }

    private static void Validate(DomainConfiguration configuration)
    {
      Core.Log.Info(Strings.LogValidatingConfiguration);
      if (configuration.Builders.Count > 0)
        foreach (Type type in configuration.Builders) {
          ValidationResult vr = Validator.ValidateBuilder(type);
          if (!vr.Success)
            throw new DomainBuilderException(vr.Message);
        }
    }

    private static void BuildStorage()
    {
      Domain domain = new Domain();
      domain.Configuration = BuildingScope.Context.Configuration;
      domain.Model = BuildingScope.Context.Model;
      domain.NameProvider = BuildingScope.Context.NameProvider;
      BuildingScope.Context.Domain = domain;
    }

    private static void BuildKeyProviders()
    {
      Log.Info(Strings.LogBuildingKeyProviders);
      Registry<HierarchyInfo, IKeyProvider> providers = BuildingScope.Context.Domain.KeyProviders;
      foreach (HierarchyInfo hierarchy in BuildingScope.Context.Model.Hierarchies) {
        try {
          IKeyProvider keyProvider = (IKeyProvider)Activator.CreateInstance(hierarchy.KeyProvider);
          providers.Register(hierarchy, keyProvider);
        }
        catch (Exception ex) {
          Log.Error(ex.ToString());
        }
      }
      providers.Lock();
    }

    private static void BuildHandlerProvider()
    {
      Log.Info(Strings.LogBuildingHandlerProvider);
      Domain domain = BuildingScope.Context.Domain;
      lock (pluginManager) {
        string protocol = domain.Configuration.ConnectionInfo.Protocol;
        Type handlerProviderType = pluginManager[new HandlerProviderAttribute(protocol)];
        if (handlerProviderType==null)
          throw new DomainBuilderException(
            string.Format(Strings.ExStorageProviderNotFound,
              protocol,
              Environment.CurrentDirectory));
        try {
          Delegate costructorDelegate = DelegateHelper.CreateClassConstructorDelegate(handlerProviderType);
          domain.HandlerProvider = (HandlerProvider)costructorDelegate.DynamicInvoke();
          domain.Handler = domain.HandlerProvider.GetHandler<DomainHandler>();
          domain.Handler.Domain = domain;
          domain.Handler.Build();
        }
        catch (Exception ex) {
          Log.Error(ex.ToString());
        }
      }
    }
  }
}