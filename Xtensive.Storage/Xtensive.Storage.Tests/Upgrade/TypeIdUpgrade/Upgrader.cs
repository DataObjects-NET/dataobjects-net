// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.29

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Tests.Upgrade.TypeIdUpgrade
{
  public class Upgrader : UpgradeHandler
  {
    private static bool isEnabled = false;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable()
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      return new Disposable(_ => {
        isEnabled = false;
      });
    }

    public override bool IsEnabled
    {
      get
      {
        return isEnabled;
      }
    }

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      hints.Add(new RemoveTypeHint(typeof(Model.Employee).FullName));
    }
  }
}