// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.30

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Upgrade.Hints;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Upgrade context.
  /// </summary>
  public sealed class UpgradeContext : Context<UpgradeScope>
  {
    /// <summary>
    /// Gets the current <see cref="UpgradeContext"/>.
    /// </summary>
    public static UpgradeContext Current
    {
      get { return UpgradeScope.CurrentContext; }
    }

    /// <summary>
    /// Gets the current <see cref="UpgradeContext"/>, or throws <see cref="InvalidOperationException"/>, if active context is not found.
    /// </summary>
    /// <returns>Current context.</returns>
    /// <exception cref="InvalidOperationException">Active context is not found.</exception>
    public static UpgradeContext Demand()
    {
      var currentContext = Current;
      if (currentContext==null)        
        throw new InvalidOperationException(
          string.Format(Strings.ActiveXIsNotFound, typeof(UpgradeContext)));
      return currentContext;
    }

    /// <summary>
    /// Gets the upgrade hints.
    /// </summary>
    public SetSlim<UpgradeHint> Hints { get; private set;}

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return UpgradeScope.CurrentContext==this; }
    }

    /// <inheritdoc/>
    protected override UpgradeScope CreateActiveScope()
    {
      return new UpgradeScope(this);
    }


    // Constructors.

    internal UpgradeContext()
    {
      Hints = new SetSlim<UpgradeHint>();
    }
  }
}