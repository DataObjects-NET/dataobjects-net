// Copyright (C) 2009 Xtensive LLC.
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
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Metadata;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Stored;
using Xtensive.Storage.Resources;
using M = Xtensive.Storage.Metadata;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// <see cref="UpgradeHandler"/> implementation 
  /// for <see cref="Xtensive.Storage"/> assembly.
  /// </summary>
  public sealed class SystemUpgradeHandler : UpgradeHandler
  {
    /// <inheritdoc/>
    public override void OnStage()
    {
      var context = UpgradeContext.Demand();
      var upgradeMode = context.OriginalConfiguration.UpgradeMode;
      switch (context.Stage) {
      case UpgradeStage.Validation:
        CheckMetadata();
        ExtractDomainModel();
        break;
      case UpgradeStage.Upgrading:
        UpdateMetadata();
        break;
      case UpgradeStage.Final:
        if (upgradeMode == DomainUpgradeMode.Recreate
          || upgradeMode == DomainUpgradeMode.Legacy)
          UpdateMetadata();
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
      var domainModel = Domain.Demand().Model;
      Query<M.Type>.All.Apply(type => type.Remove());
      Session.Demand().Persist();
      domainModel.Types
        .Where(type => type.TypeId!=TypeInfo.NoTypeId)
        .Apply(type => new M.Type(type.TypeId, type.UnderlyingType.GetFullName()));
    }

    private void UpdateDomainModel()
    {
      var domainModel = Domain.Demand().Model;
      var modelHolder = Query<Extension>.All
        .SingleOrDefault(extension => extension.Name==WellKnown.DomainModelExtensionName);
      if (modelHolder == null)
        modelHolder = new Extension(WellKnown.DomainModelExtensionName);
      using (var writer = new StringWriter()) {
        var serializer = new XmlSerializer(typeof (StoredDomainModel));
        serializer.Serialize(writer, domainModel.ToStoredModel());
        modelHolder.Text = writer.GetStringBuilder().ToString();
      }
    }

    private Pair<IUpgradeHandler, M.Assembly>[] GetAssemblies()
    {
      var context = UpgradeContext.Current;

      var oldAssemblies = Query<M.Assembly>.All.ToArray();
      var oldAssemblyByName = new Dictionary<string, M.Assembly>();
      foreach (var oldAssembly in oldAssemblies)
        oldAssemblyByName.Add(oldAssembly.Name, oldAssembly);
      var oldNames = oldAssemblies.Select(a => a.Name);
      
      var handlers = context.UpgradeHandlers.Values.ToArray();
      var handlerByName = new Dictionary<string, IUpgradeHandler>();
      foreach (var handler in handlers)
        handlerByName.Add(handler.AssemblyName, handler);
      var names = handlers.Select(a => a.AssemblyName);
      
      var commonNames = names.Intersect(oldNames);
      var addedNames = names.Except(commonNames);
      var removedNames = oldNames.Except(commonNames);

      return 
        addedNames.Select(n => new Pair<IUpgradeHandler, M.Assembly>(handlerByName[n], null))
          .Concat(commonNames.Select(n => new Pair<IUpgradeHandler, M.Assembly>(handlerByName[n], oldAssemblyByName[n])))
          .Concat(removedNames.Select(n => new Pair<IUpgradeHandler, M.Assembly>(null, oldAssemblyByName[n])))
          .ToArray();
    }

    private void ExtractDomainModel()
    {
      var context = UpgradeContext.Demand();
      var modelHolder = Query<Extension>.All
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