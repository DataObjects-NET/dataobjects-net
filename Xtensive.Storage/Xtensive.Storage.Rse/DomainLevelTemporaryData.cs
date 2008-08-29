// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Rse;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Domain saved data context.
  /// </summary>
  public class DomainLevelTemporaryData : Context<DomainLevelTemporaryDataScope>, INamedValueCollection
  {
    private readonly NamedValueCollection values;

    /// <summary>
    /// Gets the current <see cref="DomainLevelTemporaryData"/> instance.
    /// </summary>
    public static DomainLevelTemporaryData Current
    {
      [DebuggerStepThrough]
      get { return DomainLevelTemporaryDataScope.CurrentContext; }
    }

    /// <inheritdoc/>
    public object Get(string name)
    {
      return values.Get(name);
    }

    /// <inheritdoc/>
    public void Set(string name, object source)
    {
      values.Set(name, source);
    }

    #region IContext<...> methods

    /// <inheritdoc/>
    public override bool IsActive
    {
      [DebuggerStepThrough]
      get { return Current==this; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override DomainLevelTemporaryDataScope CreateActiveScope()
    {
      return new DomainLevelTemporaryDataScope(this);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DomainLevelTemporaryData()
    {
      values = new NamedValueCollection();
    }
  }
}