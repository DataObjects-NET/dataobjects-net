// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.30

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Orm.Upgrade
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
    internal UpgradeScope(UpgradeContext context) 
      : base(context)
    {
    }
  }
}