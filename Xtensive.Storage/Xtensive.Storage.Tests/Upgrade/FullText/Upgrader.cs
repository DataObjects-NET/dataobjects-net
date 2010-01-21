// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.21

using System;
using System.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Tests.Upgrade.FullText
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

    protected override void AddUpgradeHints()
    {
      var context = UpgradeContext.Current;
      switch (runningVersion) {
        case "Version1":
          break;
        case "Version2":
          break;
        case "Version3":
          break;
        case "Version4":
          context.Hints.Add(new RenameFieldHint(typeof (Model.Version4.Article), "Content", "Text"));
          break;
        case "Version5":
          context.Hints.Add(new RenameTypeHint("Xtensive.Storage.Tests.Upgrade.FullText.Model.Version4.Article", typeof(Model.Version5.Book)));
          break;
        case "Version6":
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}