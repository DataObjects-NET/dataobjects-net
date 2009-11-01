// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.29

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Ensures all the <see cref="IDisposable"/> objects added to it are disposed
  /// on disposal of <see cref="DisposableSet"/> instance.
  /// </summary>
  /// <remarks>
  ///   <note>
  ///     <see cref="IDisposable.Dispose"/> methods are invoked in backward order.
  /// </note>
  /// </remarks>
  public sealed class DisposableSet: IDisposable
  {
    private SetSlim<IDisposable> set;
    private List<IDisposable> list;

    /// <summary>
    /// Adds an <see cref="IDisposable"/> object to the set.
    /// </summary>
    /// <param name="disposable">The object to add.</param>
    /// <returns><see langword="True"/>, if object is successfully added;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Add(IDisposable disposable)
    {
      if (disposable==null)
        return false;
      EnsureInitialized();
      if (set.Add(disposable)) {
        list.Add(disposable);
        return true;
      }
      else
        return false;
    }

    private void EnsureInitialized()
    {
      if (set==null) {
        set = new SetSlim<IDisposable>();
        list = new List<IDisposable>();
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="initialContent">The initial content.</param>
    public DisposableSet(IEnumerable initialContent)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(initialContent, "initialContent");
      foreach (object o in initialContent)
        Add(o as IDisposable);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisposableSet()
    {
    }

    // Destructor

    private void Dispose(bool disposing)
    {
      try {
        if (list==null)
          return;
        using (var a = new ExceptionAggregator()) {
          for (int i = list.Count-1; i>=0; i--)
            a.Execute(d => d.Dispose(), list[i]);
        }
      }
      finally {
        set = null;
        list = null;
      }
    }

    /// <see cref="ClassDocTemplate.Dispose"/>
    void IDisposable.Dispose() 
    {
      Dispose(true);
      GC.SuppressFinalize(this); 
    }

    /// <see cref="ClassDocTemplate.Dtor"/>
    ~DisposableSet()
    {
      Dispose(false);
    }
  }
}