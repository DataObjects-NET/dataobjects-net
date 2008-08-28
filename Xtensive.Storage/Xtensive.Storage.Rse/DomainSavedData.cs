// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Storage.Rse;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Domain saved data context.
  /// </summary>
  public class DomainSavedData : Context<DomainSavedDataScope>, INamedValueCollection
  {
    private readonly NamedValueCollection values;

    /// <summary>
    /// Gets the current <see cref="DomainSavedData"/>.
    /// </summary>
    public static DomainSavedData Current
    {
      [DebuggerStepThrough]
      get { return DomainSavedDataScope.CurrentContext; }
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      [DebuggerStepThrough]
      get { return Current==this; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override DomainSavedDataScope CreateActiveScope()
    {
      return new DomainSavedDataScope(this);
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


    // Constructors.

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session"><see cref="Session"/> property value.</param>
    public DomainSavedData(DomainBound session)
    {
      values = session.Domain.SavedValues;
    }
  }
}