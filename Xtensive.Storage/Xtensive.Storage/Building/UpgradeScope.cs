// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.30

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// The scope for <see cref="UpgradeContext"/>.
  /// </summary>
  public sealed class UpgradeScope : Scope<UpgradeContext>
  {
    /// <summary>
    /// Gets the context.
    /// </summary>
    public new static UpgradeContext Context
    {
      get { return CurrentContext; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="upgradeContext">The context to activate.</param>
    public UpgradeScope(UpgradeContext upgradeContext)
      : base(upgradeContext)
    {
    }
  }
}