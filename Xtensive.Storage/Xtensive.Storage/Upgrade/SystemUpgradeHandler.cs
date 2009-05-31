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
using Xtensive.Storage.Building;
using Xtensive.Storage.Metadata;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Stored;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Upgrade.Hints;
using M=Xtensive.Storage.Metadata;

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
      var context = UpgradeContext.Current;
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
        if (upgradeMode == DomainUpgradeMode.Recreate)
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
        var h = pair.First;
        var a = pair.Second;
        if (h==null)
          throw new DomainBuilderException(string.Format(
            Strings.ExNoUpgradeHandlerIsFoundForAssemblyXVersionY,
            a.Name, a.Version));
        if (a==null) {
          if (!h.CanUpgradeFrom(null))
            throw new DomainBuilderException(string.Format(
              Strings.ExUpgradeOfAssemblyXFromVersionYToZIsNotSupported,
              h.AssemblyName, Strings.ZeroAssemblyVersion, h.AssemblyVersion));
          else
            continue;
        }
        if (!h.CanUpgradeFrom(a.Version))
          throw new DomainBuilderException(string.Format(
            Strings.ExUpgradeOfAssemblyXFromVersionYToZIsNotSupported,
            a.Name, a.Version, h.AssemblyVersion));
      }
    }

    /// <exception cref="DomainBuilderException">Impossible to upgrade all assemblies.</exception>
    private void UpdateAssemblies()
    {
      foreach (var pair in GetAssemblies()) {
        var h = pair.First;
        var a = pair.Second;
        if (h==null)
          throw new DomainBuilderException(string.Format(
            Strings.ExNoUpgradeHandlerIsFoundForAssemblyXVersionY,
            a.Name, a.Version));
        if (a==null) {
          a = new M.Assembly(h.AssemblyName) {
            Version = h.AssemblyVersion
          };
          Log.Info(Strings.LogMetadataAssemblyCreatedX, a);
        }
        else {
          var oldVersion = a.Version;
          a.Version = h.AssemblyVersion;
          Log.Info(Strings.LogMetadataAssemblyUpdatedXFromVersionYToZ, a, oldVersion, a.Version);
        }
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private void UpdateTypes()
    {
      var context = UpgradeContext.Demand();
      var typeByName = Query<M.Type>.All.ToDictionary(type => type.Name);
      var renamedTypes = new Dictionary<int, string>();
      foreach (var hint in context.Hints.OfType<RenameTypeHint>()) {
        M.Type type;
        if (!typeByName.TryGetValue(hint.OldType, out type))
          throw new DomainBuilderException(string.Format(
              Strings.ExTypeWithNameXIsNotFoundInMetadata, hint.OldType));
        var newName = hint.NewType.GetFullName();
        renamedTypes.Add(type.Id, newName);
        Log.Info(Strings.LogMetadataTypeRenamedXToY, hint.OldType, newName);
        type.Remove();
      }
      Session.Current.Persist();
      renamedTypes.Apply(idNamePair => new M.Type(idNamePair.Key, idNamePair.Value));
    }

    private void UpdateDomainModel()
    {
      var domainModel = Domain.Demand().Model;
      var modelHolder = Query<Extension>.All
        .SingleOrDefault(extension => extension.Name==StorageWellKnown.DomainModelExtension);
      if (modelHolder == null)
        modelHolder = new Extension(StorageWellKnown.DomainModelExtension);
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
        .SingleOrDefault(e => e.Name==StorageWellKnown.DomainModelExtension);
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