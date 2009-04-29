// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.30

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// The scope for <see cref="UpgradeContext"/>.
  /// </summary>
  public sealed class UpgradeScope : Scope<UpgradeContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static UpgradeContext CurrentContext
    {
      get { return Scope<UpgradeContext>.CurrentContext; }
    }

    
    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public UpgradeScope(UpgradeContext context) 
      : base(context)
    {
    }
  }
}