// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.29

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Abstract base class for temporary data storages.
  /// </summary>
  /// <typeparam name="TScope">The type of the scope.</typeparam>
  public abstract class TemporaryDataBase<TScope>: Context<TScope>, 
    INamedValueCollection
    where TScope: class, IDisposable
  {
    private readonly NamedValueCollection values;

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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected TemporaryDataBase()
    {
      values = new NamedValueCollection();
    }
  }
}