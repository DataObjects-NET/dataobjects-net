// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Metadata;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Reflection;
using Type = Xtensive.Orm.Metadata.Type;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// <see cref="UpgradeHandler"/> implementation 
  /// for <see cref="Xtensive.Orm"/> assembly.
  /// </summary>
  public sealed class SystemUpgradeHandler : UpgradeHandler
  {
    /// <inheritdoc/>
    public override bool IsEnabled
    {
      get
      {
        // Enabled just for Xtensive.Orm
        return Assembly==GetType().Assembly;
      }
    }

    public override void OnBeforeStage()
    {
      base.OnBeforeStage();

      if (UpgradeContext.Stage!=UpgradeStage.Upgrading)
        return;

      CheckMetadata();
      ExtractTypeIds();
      ExtractDomainModel();
    }

    /// <inheritdoc/>
    public override void OnStage()
    {
      var context = UpgradeContext;
      var upgradeMode = context.Configuration.UpgradeMode;
      var session = Session.Demand();
      var builder = new TypeIdBuilder(session.Domain, context.TypeIdProvider);

      switch (context.Stage) {
        case UpgradeStage.Upgrading:
          // Perform or PerformSafely
          builder.BuildTypeIds();
          UpdateMetadata(session);
          break;
        case UpgradeStage.Final:
          if (upgradeMode.IsUpgrading()) {
            // Recreate, Perform or PerformSafely
            builder.BuildTypeIds();
            UpdateMetadata(session);
          }
          else if (upgradeMode.IsLegacy()) {
            // LegacySkip and LegacyValidate
            builder.BuildTypeIds();
          }
          else {
            // Skip and Validate
            ExtractTypeIds();
            builder.BuildTypeIds();
          }
          break;
        default:
          throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    private void CheckMetadata()
    {
      CheckAssemblies();
    }

    private void UpdateMetadata(Session session)
    {
      UpdateAssemblies(session);
      UpdateTypes(session);
      UpdateDomainModel(session);
      session.SaveChanges();
    }

    /// <exception cref="DomainBuilderException">Impossible to upgrade all assemblies.</exception>
    private void CheckAssemblies()
    {
      foreach (var pair in GetAssemblies()) {
        var handler = pair.First;
        var assembly = pair.Second;
        if (handler==null)
          throw HandlerNotFound(assembly);
        if (assembly==null) {
          if (!handler.CanUpgradeFrom(null))
            throw HandlerCanNotUpgrade(handler, Strings.ZeroAssemblyVersion);
        }
        else {
          if (!handler.CanUpgradeFrom(assembly.Version))
            throw HandlerCanNotUpgrade(handler, assembly.Version);
        }
      }
    }

    private static DomainBuilderException HandlerCanNotUpgrade(IUpgradeHandler handler, string sourceVersion)
    {
      return new DomainBuilderException(string.Format(
        Strings.ExUpgradeOfAssemblyXFromVersionYToZIsNotSupported,
        handler.AssemblyName, sourceVersion, handler.AssemblyVersion));
    }

    private static DomainBuilderException HandlerNotFound(AssemblyMetadata assembly)
    {
      return new DomainBuilderException(string.Format(
        Strings.ExNoUpgradeHandlerIsFoundForAssemblyXVersionY,
        assembly.Name, assembly.Version));
    }

    private void UpdateAssemblies(Session session)
    {
      session.Query.All<Assembly>().Remove();
      session.SaveChanges();

      foreach (var pair in GetAssemblies()) {
        var handler = pair.First;
        var assembly = pair.Second;
        if (handler==null)
          throw HandlerNotFound(assembly);
        new Assembly(handler.AssemblyName) {Version = handler.AssemblyVersion};
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private void UpdateTypes(Session session)
    {
      var domainModel = session.Domain.Model;

      session.Query.All<Type>().Remove();
      session.SaveChanges();

      domainModel.Types
        .Where(type => type.IsEntity && type.TypeId!=TypeInfo.NoTypeId)
        .ForEach(type => new Type(type.TypeId, type.UnderlyingType.GetFullName()));
    }

    private void UpdateDomainModel(Session session)
    {
      var domainModel = session.Domain.Model;
      var modelHolder = session.Query.All<Extension>()
        .SingleOrDefault(extension => extension.Name==WellKnown.DomainModelExtensionName);
      if (modelHolder==null)
        modelHolder = new Extension(WellKnown.DomainModelExtensionName);
      using (var writer = new StringWriter()) {
        domainModel.ToStoredModel().Serialize(writer);
        modelHolder.Text = writer.ToString();
      }
    }

    private IEnumerable<Pair<IUpgradeHandler, AssemblyMetadata>> GetAssemblies()
    {
      var extractedAssemblies = UpgradeContext.WorkerResult.Assemblies;
      var oldAssemblies = extractedAssemblies!=null
        ? extractedAssemblies.ToDictionary(a => a.Name)
        : new Dictionary<string, AssemblyMetadata>();

      var oldNames = oldAssemblies.Keys.ToList();

      var handlers = UpgradeContext.UpgradeHandlers.Values.ToDictionary(h => h.AssemblyName);
      if (oldNames.Contains("Xtensive.Storage"))
        handlers["Xtensive.Storage"] = new SystemUpgradeHandler();
      var handledAssemblyNames = handlers.Keys;

      var commonNames = handledAssemblyNames.Intersect(oldNames).ToList();
      var addedNames = handledAssemblyNames.Except(commonNames);
      var removedNames = oldNames.Except(commonNames);

      return
        addedNames.Select(n => new Pair<IUpgradeHandler, AssemblyMetadata>(handlers[n], null))
          .Concat(commonNames.Select(n => new Pair<IUpgradeHandler, AssemblyMetadata>(handlers[n], oldAssemblies[n])))
          .Concat(removedNames.Select(n => new Pair<IUpgradeHandler, AssemblyMetadata>(null, oldAssemblies[n])))
          .ToArray();
    }

    private void ExtractDomainModel()
    {
      var context = UpgradeContext;

      var modelHolder = context.WorkerResult.Extensions
        .SingleOrDefault(e => e.Name==WellKnown.DomainModelExtensionName);

      if (modelHolder==null) {
        Log.Info(Strings.LogDomainModelIsNotFoundInStorage);
        return;
      }

      StoredDomainModel model = null;
      using (var reader = new StringReader(modelHolder.Value))
        try {
          model = StoredDomainModel.Deserialize(reader);
          model.UpdateReferences();
        }
        catch (Exception e) {
          Log.Warning(e, Strings.LogFailedToExtractDomainModelFromStorage);
        }

      context.ExtractedDomainModel = model;
    }

    private void ExtractTypeIds()
    {
      var context = UpgradeContext;
      context.ExtractedTypeMap = context.WorkerResult.Types.ToDictionary(t => t.Name, t => t.Id);
    }
  }
}