// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.29

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  public class TemporaryDataBase: Context<TemporaryDataScopeBase>, INamedValueCollection
  {
    private readonly NamedValueCollection values;

    /// <summary>
    /// Gets the current <see cref="TemporaryDataBase"/> instance.
    /// </summary>
    public static TemporaryDataBase Current
    {
      [DebuggerStepThrough]
      get { return TemporaryDataScopeBase.CurrentContext; }
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
    protected override TemporaryDataScopeBase CreateActiveScope()
    {
      return new TemporaryDataScopeBase(this);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TemporaryDataBase()
    {
      values = new NamedValueCollection();
    }
  }
}