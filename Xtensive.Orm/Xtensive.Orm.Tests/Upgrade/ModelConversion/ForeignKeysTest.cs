// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.11

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using Xtensive.Storage.Model;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class ForeignKeysTest : AutoBuildTest
  {
    private Domain domain;

    [Test]
    public void MainTest()
    {
      if (domain!=null)
        domain.DisposeSafely();
      using (TargetSchemaVerifier.Enable("1")) {
        BuildDomain(BuildConfiguration());
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.ForeignKeyMode = ForeignKeyMode.Reference;
      var type = typeof (Tests.Model.Association.Root);
      configuration.Types.Register(type.Assembly, type.Namespace);
      return configuration;
    }
  }

  [Serializable]
  public class TargetSchemaVerifier : UpgradeHandler
  {
    private static bool isEnabled = false;
    private static string runningVersion;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(string version)
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      runningVersion = version;
      return new Disposable(_ => {
        isEnabled = false;
        runningVersion = null;
      });
    }

    public override bool IsEnabled {
      get {
        return isEnabled;
      }
    }
    
    protected override string DetectAssemblyVersion()
    {
      return runningVersion;
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnSchemaReady()
    {
      base.OnSchemaReady();
      var context = UpgradeContext.Demand();
      if (context.Stage!=UpgradeStage.Final)
        return;
      if (ConcreteTableSchemaModifier.IsEnabled 
        || SingleTableSchemaModifier.IsEnabled)
        return;
      var targetSchema = context.SchemaHints.TargetModel as StorageInfo;
      var tableA = targetSchema.Tables["A"];
      var tableB = targetSchema.Tables["B"];
      var tableC = targetSchema.Tables["C"];
      var tableD = targetSchema.Tables["D"];
      var tableE = targetSchema.Tables["E"];
      var tableF = targetSchema.Tables["F"];
      var tableG = targetSchema.Tables["G"];
      var tableH = targetSchema.Tables["H"];
      var tableAE = targetSchema.Tables["A-ZeroToMany-E"];
      var tableAG = targetSchema.Tables["A-ManyToManyMaster-G"];

      // ZeroToOne (A -> B)
      Assert.AreEqual(0, tableB.ForeignKeys.Count);
      Assert.AreEqual(1, tableA.ForeignKeys.Count(fk => fk.PrimaryKey==tableB.PrimaryIndex));
      // OneToOnePaired (A -> C, C -> A)
      Assert.AreEqual(1, tableA.ForeignKeys.Count(fk => fk.PrimaryKey==tableC.PrimaryIndex));
      Assert.AreEqual(2, tableC.ForeignKeys.Count(fk => fk.PrimaryKey==tableA.PrimaryIndex));
      // ManyToOnePaired (A -> D)
      Assert.AreEqual(1, tableA.ForeignKeys.Count(fk => fk.PrimaryKey==tableD.PrimaryIndex));
      Assert.AreEqual(0, tableD.ForeignKeys.Count);
      // OneToManyPaired (A -> F)
      Assert.AreEqual(0, tableA.ForeignKeys.Count(fk => fk.PrimaryKey==tableF.PrimaryIndex));
      Assert.AreEqual(1, tableF.ForeignKeys.Count(fk => fk.PrimaryKey==tableA.PrimaryIndex));
      // ZeroToOne (AE -> A, AE -> E)
      Assert.AreEqual(1, tableAE.ForeignKeys.Count(fk => fk.PrimaryKey==tableA.PrimaryIndex));
      Assert.AreEqual(1, tableAE.ForeignKeys.Count(fk => fk.PrimaryKey==tableE.PrimaryIndex));
      // ManyToMany (AG -> A, AG -> G)
      Assert.AreEqual(1, tableAG.ForeignKeys.Count(fk => fk.PrimaryKey==tableA.PrimaryIndex));
      Assert.AreEqual(1, tableAG.ForeignKeys.Count(fk => fk.PrimaryKey==tableG.PrimaryIndex));
      // ManyToOneSelfPaired (H -> H)
      Assert.AreEqual(1, tableH.ForeignKeys.Count(fk => fk.PrimaryKey==tableH.PrimaryIndex));
    }

    public override void OnUpgrade()
    {
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      return true;
    }
  }
}
