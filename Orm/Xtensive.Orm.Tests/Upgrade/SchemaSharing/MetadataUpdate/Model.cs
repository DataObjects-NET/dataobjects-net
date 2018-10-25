// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.04.03

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.MetadataUpdate.Model
{
  namespace Part1
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    [Recycled]
    public class RecycledTestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    public class NewTestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  namespace Part2
  {
    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    [Recycled]
    public class RecycledTestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    public class NewTestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  namespace Part3
  {
    [HierarchyRoot]
    public class TestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    [Recycled]
    public class RecycledTestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    public class NewTestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  namespace Part4
  {
    [HierarchyRoot]
    public class TestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    [Recycled]
    public class RecycledTestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    public class NewTestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    private enum Stage
    {
      Initial,
      AfterUpgrading,
      Final
    }

    private List<Type> recycledTypes;
    private List<Type> newTypes;
    private List<Type> initialTypes;
    private Stage internalStage;

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      initialTypes = new List<Type>();
      initialTypes.Add(typeof (Part1.TestEntity1));
      initialTypes.Add(typeof (Part2.TestEntity2));
      initialTypes.Add(typeof (Part3.TestEntity3));
      initialTypes.Add(typeof (Part4.TestEntity4));

      recycledTypes = new List<Type>();
      recycledTypes.Add(typeof (Part1.RecycledTestEntity1));
      recycledTypes.Add(typeof (Part2.RecycledTestEntity2));
      recycledTypes.Add(typeof (Part3.RecycledTestEntity3));
      recycledTypes.Add(typeof (Part4.RecycledTestEntity4));

      newTypes = new List<Type>();
      newTypes.Add(typeof (Part1.NewTestEntity1));
      newTypes.Add(typeof (Part2.NewTestEntity2));
      newTypes.Add(typeof (Part3.NewTestEntity3));
      newTypes.Add(typeof (Part4.NewTestEntity4));

      internalStage = Stage.Initial;
    }

    public override void OnBeforeStage()
    {
      base.OnBeforeStage();
      var metadataMapping = new MetadataMapping(UpgradeContext.Services.StorageDriver, UpgradeContext.Services.NameBuilder);
      var metadataTasks = UpgradeContext.Services.MappingResolver.GetMetadataTasks();
      var executor = new Providers.SqlExecutor(UpgradeContext.Services.StorageDriver, UpgradeContext.Services.Connection);
      var metadataSet = new MetadataSet();
      var isRecreate = UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.Recreate;
      var extractor = new MetadataExtractor(metadataMapping, executor);
      foreach (var sqlExtractionTask in metadataTasks) {
        if (!isRecreate) {
          extractor.ExtractAssemblies(metadataSet, sqlExtractionTask);
          extractor.ExtractTypes(metadataSet, sqlExtractionTask);
          extractor.ExtractExtensions(metadataSet, sqlExtractionTask);
        }
      }

      ValidateMetadataTables(metadataSet, internalStage, isRecreate);
      ValidateUserTables(metadataSet, internalStage, isRecreate);

      if (UpgradeContext.Stage==UpgradeStage.Upgrading)
        internalStage = Stage.AfterUpgrading;
      else
        internalStage = Stage.Final;
    }

    public override void OnStage()
    {
      base.OnStage();
      // here, for every custom handler or not system upgrade handler, we have updated metadata
      // also here we have a session and selected storage node for it
      // so we can finally validate that the metadata of the building node is saved into right database/scheme pair
      var metadataMapping = new MetadataMapping(UpgradeContext.Services.StorageDriver, UpgradeContext.Services.NameBuilder);
      var metadataTasks = UpgradeContext.Services.MappingResolver.GetMetadataTasks();
      var executor = new Providers.SqlExecutor(UpgradeContext.Services.StorageDriver, UpgradeContext.Services.Connection);
      var metadataSet = new MetadataSet();
      var isRecreate = UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.Recreate;
      var extractor = new MetadataExtractor(metadataMapping, executor);

      foreach (var sqlExtractionTask in metadataTasks) {
        extractor.ExtractAssemblies(metadataSet, sqlExtractionTask);
        extractor.ExtractTypes(metadataSet, sqlExtractionTask);
        extractor.ExtractExtensions(metadataSet, sqlExtractionTask);
      }

      ValidateMetadataTables(metadataSet, internalStage, isRecreate);
      ValidateUserTables(metadataSet, internalStage, isRecreate);
    }

    private void ValidateMetadataTables(MetadataSet metadataSet, Stage stage, bool isRecreate)
    {
      if (stage==Stage.Initial && isRecreate) {
        Assert.That(metadataSet.Assemblies.Count, Is.EqualTo(0));
        Assert.That(metadataSet.Extensions.Count, Is.EqualTo(0));
        Assert.That(metadataSet.Types.Count, Is.EqualTo(0));
      }
      else {
        Assert.That(metadataSet.Assemblies.Any(a => a.Name=="Xtensive.Orm"), Is.True);

        Assert.That(metadataSet.Types.Any(t => t.Name=="Xtensive.Orm.Metadata.Assembly"), Is.True);
        Assert.That(metadataSet.Types.Any(t => t.Name=="Xtensive.Orm.Metadata.Extension"), Is.True);
        Assert.That(metadataSet.Types.Any(t => t.Name=="Xtensive.Orm.Metadata.Type"), Is.True);
        if (!UpgradeContext.Configuration.IsMultidatabase)
          Assert.That(metadataSet.Extensions.Count, Is.EqualTo(1));
        else
          Assert.That(metadataSet.Extensions.Count, Is.EqualTo(2));
      }
    }

    private void ValidateUserTables(MetadataSet metadataSet, Stage stage, bool isRecreate)
    {
      var initialTypesAreAvailable = stage!=Stage.Initial || !isRecreate;
      var recycledAreAvailable = stage==Stage.AfterUpgrading;
      var newTypesAvailable = stage!=Stage.Initial;

      if (stage==Stage.Initial && isRecreate)
        Assert.That(metadataSet.Assemblies.Any(a => a.Name=="Xtensive.Orm.Tests"), Is.False);
      else
        Assert.That(metadataSet.Assemblies.Any(a => a.Name=="Xtensive.Orm.Tests"), Is.True);

      foreach (var initialType in initialTypes)
        Assert.That(metadataSet.Types.Any(t => t.Name==initialType.FullName), Is.EqualTo(initialTypesAreAvailable));

      foreach (var recycledType in recycledTypes)
        Assert.That(metadataSet.Types.Any(t => t.Name==recycledType.FullName), Is.EqualTo(recycledAreAvailable));

      foreach (var newType in newTypes)
        Assert.That(metadataSet.Types.Any(t => t.Name==newType.FullName), Is.EqualTo(newTypesAvailable));
    }
  }
}
