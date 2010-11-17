// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Services;
using Xtensive.Orm.Upgrade;
using M1 = Xtensive.Orm.Tests.Issues.Issue_0841_HintGeneratorBug3.Model.Version1;
using M2 = Xtensive.Orm.Tests.Issues.Issue_0841_HintGeneratorBug3.Model.Version2;

namespace Xtensive.Orm.Tests.Issues.Issue_0841_HintGeneratorBug3
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

    protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
    {
#pragma warning disable 612,618
      // var hints = GetTypeRenameHints("Version1", "Version2");
      hints.Add(new RenameTypeHint(typeof (M1.Base).FullName, typeof (M2.Base)));
      hints.Add(new RenameTypeHint(typeof (M1.Derived).FullName, typeof (M2.Derived)));
#pragma warning restore 612,618
    }

    public override void  OnStage()
    {
      if (runningVersion!="2")
        return;
      var session = Session.Demand();
      var directSql = session.Services.Demand<DirectSqlAccessor>();
      if (!directSql.IsAvailable)
        return; // IMDB, so there is nothing to uprade
      if (UpgradeContext.Stage==UpgradeStage.Initializing) {
        // Relying on Metadata.Type, because
        // only system types are registered in model @ this stage.
        int baseTypeId = Query.All<Metadata.Type>()
          .Where(t => t.Name==typeof(M1.Base).FullName).Single().Id;
        int derivedTypeId = Query.All<Metadata.Type>()
          .Where(t => t.Name==typeof(M1.Derived).FullName).Single().Id;
        var command = directSql.CreateCommand();
        command.CommandText = @"
          UPDATE [dbo].[Base] SET [TypeId] = {0} 
          WHERE ([Base].[TypeId] = {1});
          ".FormatWith(baseTypeId, derivedTypeId);
        command.ExecuteNonQuery();
      }
      if (UpgradeContext.Stage==UpgradeStage.Upgrading) {
        var command = directSql.CreateCommand();
        command.CommandText = @"
          UPDATE [dbo].[Base] SET [Text] = [th].[Text] FROM (
            SELECT [a].[Id], [a].[Text] FROM [dbo].[Derived] [a]) [th] 
          WHERE ([Base].[Id] = [th].[Id]);

          DELETE FROM [dbo].[Derived];
          ";
        command.ExecuteNonQuery();
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