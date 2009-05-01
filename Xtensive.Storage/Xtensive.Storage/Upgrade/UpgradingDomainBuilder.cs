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
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Builds domain in extended modes.
  /// </summary>
  public static class UpgradingDomainBuilder
  {
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
      var configuration = context.Configuration = context.OriginalConfiguration.Clone();
      context.Stage = stage;
      // Raising "Before upgrade" event
      foreach (var handler in context.UpgradeHandlers.Values)
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
      foreach (var handler in context.UpgradeHandlers.Values)
        handler.OnStage();
    }

    internal static void OnSchemasReady(StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var context = UpgradeContext.Current;
      if (context==null)
        return;
      context.SourceSchema = sourceSchema;
      context.TargetSchema = targetSchema;
      if (context.Stage==UpgradeStage.Upgrading)
        BuildSchemaHints();
    }

    internal static void OnActionsReady(NodeDifference schemaDifference, ActionSequence schemaUpgradeActions)
    {
      var context = UpgradeContext.Current;
      if (context==null)
        return;
      context.SchemaDifference = schemaDifference;
      context.SchemaUpgradeActions = schemaUpgradeActions;
    }

    private static bool TypeFilter(Type type)
    {
      var context = UpgradeContext.Current;
      var assembly = type.Assembly;
      var handlers = context.UpgradeHandlers;
      return 
        handlers.ContainsKey(assembly) 
        && handlers[assembly].IsTypeAvailable(type, context.Stage);
    }

    private static void BuildSchemaHints()
    {
      var context = UpgradeContext.Demand();
      context.SchemaHints = new HintSet(context.SourceSchema, context.TargetSchema);
      foreach (var hint in context.Hints)
        hint.Translate(context.SchemaHints);
    }

    /// <exception cref="DomainBuilderException">More then one enabled handler is provided for some assembly.</exception>
    private static void BuildUpgradeHandlers()
    {
      var context = UpgradeContext.Current;
      var handlers = new Dictionary<Assembly, IUpgradeHandler>();

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
        let handler = (IUpgradeHandler) type.Activate(ArrayUtils<Type>.EmptyArray, ArrayUtils<Type>.EmptyArray)
        where handler!=null && handler.IsEnabled
        group handler by assembly;

      // Adding user handlers
      foreach (var group in userHandlers) {
        if (group.Count()>1)
          throw new DomainBuilderException(string.Format(
            Strings.ExMoreThanOneEnabledXIsProvidedForAssemblyY, 
            typeof(IUpgradeHandler).GetShortName(), group.Key));
        handlers.Add(group.Key, group.First());
      }

      // Adding default handlers
      var assemblieswithUserHandlers = userHandlers.Select(g => g.Key);
      var assembliesWithoutUserHandler = assemblies.Except(assemblieswithUserHandlers);

      foreach (var assembly in assembliesWithoutUserHandler) {
        var handler = new UpgradeHandler {
          UpgradeContext = context
        };
        handlers.Add(assembly, handler);
      }

      // Storing thr result
      context.UpgradeHandlers = 
        new ReadOnlyDictionary<Assembly, IUpgradeHandler>(handlers, false);
    }
  }
}