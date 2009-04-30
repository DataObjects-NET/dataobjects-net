// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Builds domain in extended modes.
  /// </summary>
  public static class UpgradingDomainBuilder
  {
    private static Domain BuildUpgradeSafely(DomainConfiguration configuration)
    {
      return DomainBuilder.BuildDomain(configuration,
        SchemaUpgradeMode.UpgradeSafely,
        () => { });
    }

    /// <summary>
    /// Builds the new <see cref="Domain"/> by the specified configuration.
    /// </summary>
    /// <param name="configuration">The domain configuration.</param>
    /// <returns>Newly created <see cref="Domain"/>.</returns>
    /// <exception cref="ArgumentNullException">Parameter <paramref name="configuration"/> is null.</exception>
    /// <exception cref="DomainBuilderException">At least one error have been occurred 
    /// during storage building process.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><c>configuration.UpgradeMode</c> is out of range.</exception>
    public static Domain Build(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      var context = new UpgradeContext();
      using (context.Activate()) {
        context.OriginalConfiguration = configuration;
        context.Stage = UpgradeStage.Validation;
        BuildUpgradeHandlers();
        Stage(UpgradeStage.Validation);
        Stage(UpgradeStage.Upgrading);
        Stage(UpgradeStage.Final);
        return context.Domain;
      }
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    private static void Stage(UpgradeStage stage)
    {
      var context = UpgradeContext.Current;
      var configuration = context.Configuration = context.Configuration.Clone();
      context.Stage = stage;
      // Raising "Before upgrade" event
      foreach (var handler in context.AllUpgradeHandlers)
        handler.OnBeforeStage();

      var schemaUpgradeMode = SchemaUpgradeMode.Upgrade;
      switch (stage) {
      case UpgradeStage.Validation:
        if (configuration.UpgradeMode==DomainUpgradeMode.Recreate ||
            configuration.UpgradeMode==DomainUpgradeMode.Validate)
          return; // Nothing to do in these modes here
        schemaUpgradeMode = SchemaUpgradeMode.ValidateCompatible;
        break;
      case UpgradeStage.Upgrading:
        if (configuration.UpgradeMode==DomainUpgradeMode.Recreate ||
            configuration.UpgradeMode==DomainUpgradeMode.Validate)
          return; // Nothing to do in these modes here
        if (configuration.UpgradeMode==DomainUpgradeMode.PerformSafely)
          schemaUpgradeMode = SchemaUpgradeMode.UpgradeSafely;
        break;
      case UpgradeStage.Final:
        if (configuration.UpgradeMode==DomainUpgradeMode.Recreate)
          schemaUpgradeMode = SchemaUpgradeMode.Recreate;
        if (configuration.UpgradeMode==DomainUpgradeMode.Validate)
          schemaUpgradeMode = SchemaUpgradeMode.ValidateExact;
        break;
      default:
        throw new ArgumentOutOfRangeException("context.Stage");
      }
      using (DomainBuilder.BuildDomain(
        configuration, 
        schemaUpgradeMode, 
        OnStage, // Raising "Inside upgrade" event
        TypeFilter)) {};
    }

    private static void OnStage()
    {
      var context = UpgradeContext.Current;
      context.Domain = Domain.Demand();
      foreach (var handler in context.AllUpgradeHandlers)
        handler.OnStage();
    }

    private static bool TypeFilter(Type type)
    {
      var context = UpgradeContext.Current;
      var assembly = type.Assembly;
      bool result = false;
      foreach (var handler in context.UpgradeHandlers[assembly])
        result |= handler.IsTypeAvailable(type, context.Stage);
      return result;
    }

    private static void BuildUpgradeHandlers()
    {
      var context = UpgradeContext.Current;
      var dHandlers = new Dictionary<Assembly, IList<IUpgradeHandler>>();
      var lHandlers = new List<IUpgradeHandler>();

      var assemblies = (
        from type in context.OriginalConfiguration.Types
        let assembly = type.Assembly
        select assembly).Distinct().ToList();
      
      // Creating user handlers
      var userHandlers =
        from assembly in assemblies
        from type in assembly.GetTypes()
        where 
          (typeof (IUpgradeHandler)).IsAssignableFrom(type)
          && !type.IsAbstract
          && type.IsClass
          && type!=typeof(UpgradeHandler)
        let handler = (IUpgradeHandler) type.TypeInitializer.Invoke(null)
        where handler.IsEnabled
        group handler by assembly;

      // Adding user handlers
      foreach (var group in userHandlers) {
        var list = new ReadOnlyList<IUpgradeHandler>(group.ToList(), true);
        foreach (var handler in list) {
          handler.UpgradeContext = context;
          lHandlers.Add(handler);
        }
        dHandlers.Add(group.Key, list);
      }

      // Adding default handlers
      var assemblieswithUserHandlers = userHandlers.Select(g => g.Key);
      var assembliesWithoutUserHandler = assemblies.Except(assemblieswithUserHandlers);

      foreach (var assembly in assembliesWithoutUserHandler) {
        var handler = new UpgradeHandler {
          UpgradeContext = context
        };
        lHandlers.Add(handler);
        var list = new ReadOnlyList<IUpgradeHandler>(new List<IUpgradeHandler>() {handler}, false);
        dHandlers.Add(assembly, list);
      }

      // Storing thr result
      context.UpgradeHandlers = 
        new ReadOnlyDictionary<Assembly, IList<IUpgradeHandler>>(dHandlers, false);
      context.AllUpgradeHandlers =
        new ReadOnlyList<IUpgradeHandler>(lHandlers, false);
    }
  }
}