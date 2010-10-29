// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.21

using System;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Disposing;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.FullText
{
  public class Upgrader: UpgradeHandler
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

    public override bool IsEnabled
    {
      get { return isEnabled; }
    }

    protected override string DetectAssemblyVersion()
    {
      return runningVersion;
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      var context = UpgradeContext;
      switch (runningVersion) {
        case "Version1":
          break;
        case "Version2":
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version1.Article", typeof(Model.Version2.Article)));
          break;
        case "Version3":
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version2.Article", typeof(Model.Version3.Article)));
          break;
        case "Version4":
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version3.Article", typeof(Model.Version4.Article)));
          hints.Add(new RenameFieldHint(typeof (Model.Version4.Article), "Content", "Text"));
          break;
        case "Version5":
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version4.Article", typeof(Model.Version5.Book)));
          break;
        case "Version6":
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.FullText.Model.Version5.Book", typeof(Model.Version6.Book)));
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}