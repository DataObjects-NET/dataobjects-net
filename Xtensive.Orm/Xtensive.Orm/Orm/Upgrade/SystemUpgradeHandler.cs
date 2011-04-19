// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Metadata;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Resources;
using M = Xtensive.Orm.Metadata;
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
    public override bool IsEnabled {
      get {
        // Enabled just for Xtensive.Storage
        return Assembly==GetType().Assembly;
      }
    }

    /// <inheritdoc/>
    public override void OnStage()
    {
      var context = UpgradeContext;
      var upgradeMode = context.OriginalConfiguration.UpgradeMode;
      switch (context.Stage) {
      case UpgradeStage.Initializing:
        TypeIdBuilder.BuildTypeIds(false);
        if (upgradeMode.IsUpgrading())
          // Perform or PerformSafely
          CheckMetadata();
        ExtractDomainModel(false);
        break;
      case UpgradeStage.Upgrading:
        // Perform or PerformSafely
        TypeIdBuilder.BuildTypeIds(false);
        UpdateMetadata();
        break;
      case UpgradeStage.Final:
        if (upgradeMode.IsUpgrading()) {
          // Recreate, Perform or PerformSafely
          TypeIdBuilder.BuildTypeIds(false);
          UpdateMetadata();
        }
        else if (upgradeMode.IsLegacy()) {
          // LegacySkip and LegacyValidate
          TypeIdBuilder.BuildTypeIds(false);
        }
        else {
          // Skip and Validate
          // We need only system types to extract other TypeIds
          TypeIdBuilder.BuildTypeIds(true); 
          ExtractDomainModel(true);
          // And only after that we can build all TypeIds
          TypeIdBuilder.BuildTypeIds(false);
        }
        break;
      default:
        throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    private void CheckMetadata()
    {
      CheckAssemblies();
    }

    private void UpdateMetadata()
    {
      UpdateAssemblies();
      UpdateTypes();
      UpdateDomainModel();
    }

    /// <exception cref="DomainBuilderException">Impossible to upgrade all assemblies.</exception>
    private void CheckAssemblies()
    {
      foreach (var pair in GetAssemblies()) {
        var handler = pair.First;
        var assembly = pair.Second;
        if (handler==null)
          throw new DomainBuilderException(string.Format(
            Strings.ExNoUpgradeHandlerIsFoundForAssemblyXVersionY,
            assembly.Name, assembly.Version));
        if (assembly==null) {
          if (!handler.CanUpgradeFrom(null))
            throw new DomainBuilderException(string.Format(
              Strings.ExUpgradeOfAssemblyXFromVersionYToZIsNotSupported,
              handler.AssemblyName, Strings.ZeroAssemblyVersion, handler.AssemblyVersion));
          else
            continue;
        }
        if (!handler.CanUpgradeFrom(assembly.Version))
          throw new DomainBuilderException(string.Format(
            Strings.ExUpgradeOfAssemblyXFromVersionYToZIsNotSupported,
            assembly.Name, assembly.Version, handler.AssemblyVersion));
      }
    }

    /// <exception cref="DomainBuilderException">Impossible to upgrade all assemblies.</exception>
    private void UpdateAssemblies()
    {
      foreach (var pair in GetAssemblies()) {
        var handler = pair.First;
        var assembly = pair.Second;
        if (handler==null)
          throw new DomainBuilderException(string.Format(
            Strings.ExNoUpgradeHandlerIsFoundForAssemblyXVersionY,
            assembly.Name, assembly.Version));
        if (assembly==null) {
          assembly = new M.Assembly(handler.AssemblyName) {
            Version = handler.AssemblyVersion
          };
          Log.Info(Strings.LogMetadataAssemblyCreatedX, assembly);
        }
        else {
          var oldVersion = assembly.Version;
          assembly.Version = handler.AssemblyVersion;
          Log.Info(Strings.LogMetadataAssemblyUpdatedXFromVersionYToZ, assembly, oldVersion, assembly.Version);
        }
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private void UpdateTypes()
    {
      var session = Session.Demand();
      var domainModel = session.Domain.Model;
      session.Query.All<M.Type>().Remove();
      session.SaveChanges();
      domainModel.Types
        .Where(type => type.IsEntity && type.TypeId!=TypeInfo.NoTypeId)
        .ForEach(type => new M.Type(type.TypeId, type.UnderlyingType.GetFullName()));
    }

    private void UpdateDomainModel()
    {
      var domainModel = Domain.Demand().Model;
      var modelHolder = Session.Demand().Query.All<Extension>()
        .SingleOrDefault(extension => extension.Name==WellKnown.DomainModelExtensionName);
      if (modelHolder == null)
        modelHolder = new Extension(WellKnown.DomainModelExtensionName);
      using (var writer = new StringWriter()) {
        var serializer = new XmlSerializer(typeof (StoredDomainModel));
        serializer.Serialize(writer, domainModel.ToStoredModel());
        modelHolder.Text = writer.GetStringBuilder().ToString();
      }
    }

    private IEnumerable<Pair<IUpgradeHandler, Assembly>> GetAssemblies()
    {
      var assemblies = Session.Demand().Query.All<M.Assembly>().ToDictionary(a => a.Name);
      var oldNames = assemblies.Keys;

      var handlers = UpgradeContext.UpgradeHandlers.Values.ToDictionary(h => h.AssemblyName);
      if (oldNames.Contains("Xtensive.Storage"))
        handlers["Xtensive.Storage"] = new SystemUpgradeHandler();
      var handledAssemblyNames = handlers.Keys;
      
      var commonNames = handledAssemblyNames.Intersect(oldNames);
      var addedNames = handledAssemblyNames.Except(commonNames);
      var removedNames = oldNames.Except(commonNames);

      return 
        addedNames.Select(n => new Pair<IUpgradeHandler, M.Assembly>(handlers[n], null))
          .Concat(commonNames.Select(n => new Pair<IUpgradeHandler, M.Assembly>(handlers[n], assemblies[n])))
          .Concat(removedNames.Select(n => new Pair<IUpgradeHandler, M.Assembly>(null, assemblies[n])))
          .ToArray();
    }

    private void ExtractDomainModel(bool typeIdsOnly)
    {
      var context = UpgradeContext;
      context.ExtractedTypeMap = Session.Demand().Query.All<Type>().ToDictionary(t => t.Name, t => t.Id);
      if (typeIdsOnly)
        return;

      var modelHolder = Session.Demand().Query.All<Extension>()
        .SingleOrDefault(e => e.Name==WellKnown.DomainModelExtensionName);
      if (modelHolder == null) {
        Log.Info(Strings.LogDomainModelIsNotFoundInStorage);
        return;
      }
      StoredDomainModel model = null;
      var serializer = new XmlSerializer(typeof (StoredDomainModel));
      using (var reader = new StringReader(modelHolder.Text))
        try {
          model = (StoredDomainModel) serializer.Deserialize(reader);
          model.UpdateReferences();
        }
        catch (Exception e) {
          Log.Warning(e, Strings.LogFailedToExtractDomainModelFromStorage);
        }
      context.ExtractedDomainModel = model;
    }
  }
}