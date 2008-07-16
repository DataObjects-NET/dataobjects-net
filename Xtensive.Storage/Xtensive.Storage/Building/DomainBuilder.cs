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

      Validate(configuration);

      BuildingContext context = new BuildingContext(configuration);
      context.NameProvider = new NameProvider(configuration.NamingConvention);

      using (new BuildingScope(context)) {

        try {
          BuildModel();
          BuildDomain();
          BuildHandlerProvider();
          BuildKeyProviders();
        }
        catch (DomainBuilderException e) {          
          BuildingContext.Current.RegistError(e);
        }        

        context.EnsureBuildSucceed();
      }
      return context.Domain;
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

    private static void BuildDomain()
    {
      Domain domain = new Domain();
      domain.HandlerAccessor = new HandlerAccessor(domain);
      domain.HandlerAccessor.Configuration = BuildingScope.Context.Configuration;
      domain.HandlerAccessor.Model = BuildingScope.Context.Model;
      domain.HandlerAccessor.NameProvider = BuildingScope.Context.NameProvider;
      BuildingScope.Context.Domain = domain;
    }

    private static void BuildKeyProviders()
    {
      Log.Info(Strings.LogBuildingKeyProviders);
      Registry<HierarchyInfo, IKeyProvider> providers = BuildingScope.Context.Domain.HandlerAccessor.KeyProviders;
      foreach (HierarchyInfo hierarchy in BuildingScope.Context.Model.Hierarchies) {        
        IKeyProvider keyProvider = (IKeyProvider)Activator.CreateInstance(hierarchy.KeyProvider);
        providers.Register(hierarchy, keyProvider);        
      }
      providers.Lock();
    }

    private static void BuildHandlerProvider()
    {
      Log.Info(Strings.LogBuildingHandlerProvider);
      HandlerAccessor handlerAccessor = BuildingScope.Context.Domain.HandlerAccessor;
      lock (pluginManager) {
        string protocol = handlerAccessor.Configuration.ConnectionInfo.Protocol;
        Type handlerProviderType = pluginManager[new HandlerProviderAttribute(protocol)];

        if (handlerProviderType==null)
          throw new DomainBuilderException(
            string.Format(Strings.ExStorageProviderNotFound,
              protocol,
              Environment.CurrentDirectory));
        
        Delegate costructorDelegate = DelegateHelper.CreateClassConstructorDelegate(handlerProviderType);
        handlerAccessor.HandlerProvider = (HandlerProvider)costructorDelegate.DynamicInvoke();
        handlerAccessor.DomainHandler = handlerAccessor.HandlerProvider.GetHandler<DomainHandler>();
        handlerAccessor.DomainHandler.HandlerAccessor = handlerAccessor;
        handlerAccessor.DomainHandler.Build();        
      }
    }
  }
}
