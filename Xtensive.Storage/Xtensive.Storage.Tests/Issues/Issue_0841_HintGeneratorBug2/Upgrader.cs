// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Upgrade;
using M1 = Xtensive.Storage.Tests.Issues.Issue_0841_HintGeneratorBug2.Model.Version1;
using M2 = Xtensive.Storage.Tests.Issues.Issue_0841_HintGeneratorBug2.Model.Version2;

namespace Xtensive.Storage.Tests.Issues.Issue_0841_HintGeneratorBug2
{
  [Serializable]
  public class Upgrader : UpgradeHandler
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

    protected override void AddUpgradeHints(Core.Collections.ISet<UpgradeHint> hints)
    {
#pragma warning disable 612,618
      // var hints = GetTypeRenameHints("Version1", "Version2");
      hints.Add(new RenameTypeHint(typeof (M1.Base).FullName, typeof (M2.Base)));
      hints.Add(new RenameTypeHint(typeof (M1.Derived).FullName, typeof (M2.Derived)));
#pragma warning restore 612,618
    }

    public override void OnSchemaReady()
    {
      if (runningVersion!="2")
        return;
      string baseTableNodePath = "Tables/Base";
      string baseTableColumnPath = "Tables/Base/Columns/";
      string derivedTableNodePath = "Tables/Derived";
      string derivedTableColumnPath = "Tables/Derived/Columns/";
      if (UpgradeContext.Stage==UpgradeStage.Upgrading) {
        // Replacing Dervided type's TypeId in Base table
        // to Base type's TypeId; if there are many ancestors,
        // this action must be performed for each of them.
        UpgradeContext.SchemaHints.Add(new UpdateDataHint(
          baseTableNodePath,
          new List<IdentityPair> {
            new IdentityPair(baseTableColumnPath + "TypeId",
              UpgradeContext.ExtractedTypeMap[typeof (M1.Derived).FullName].ToString(), true)
          },
          new List<Pair<string, object>> {
            new Pair<string, object>(baseTableColumnPath + "TypeId",
              UpgradeContext.ExtractedTypeMap[typeof (M1.Base).FullName])
          }));
      }
      if (UpgradeContext.Stage==UpgradeStage.Final) {
        // Copying the data from "Text" column in "Dervided" table
        // to "Base" table
        UpgradeContext.SchemaHints.Add(new CopyDataHint(
          derivedTableNodePath,
          new List<IdentityPair> {
            new IdentityPair(
              derivedTableColumnPath + "Id", baseTableColumnPath + "Id", false)
          },
          new List<Pair<string>> {
            new Pair<string>(
              derivedTableColumnPath + "Text", baseTableColumnPath + "Text"),
          }));
      }
    }


    private static List<UpgradeHint> GetTypeRenameHints(string oldVersionSuffix, string newVersionSuffix)
    {
      var upgradeContext = UpgradeContext.Demand();
      var oldTypes = upgradeContext.ExtractedDomainModel.Types;
      var hints = new List<UpgradeHint>();
      foreach (var type in oldTypes) {
        var fullName = type.UnderlyingType;
        int lastDotIndex = fullName.LastIndexOf(".");
        if (lastDotIndex<0)
          lastDotIndex = 1;
        var ns = fullName.Substring(0, lastDotIndex);
        var name = fullName.Substring(lastDotIndex + 1);
        if (ns.EndsWith(oldVersionSuffix)) {
          string newNs = ns.Substring(0, ns.Length - oldVersionSuffix.Length) + newVersionSuffix;
          string newFullName = newNs + "." + name;
          Type newType = upgradeContext.Configuration.Types.SingleOrDefault(t => t.FullName==newFullName);
          if (newType!=null)
            hints.Add(new RenameTypeHint(fullName, newType));
        }
      }
      return hints;
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Version" + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace!=originalNamespace 
        && base.IsTypeAvailable(type, upgradeStage);
    }
  }
}