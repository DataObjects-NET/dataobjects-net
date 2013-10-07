// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.29

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;


namespace Xtensive.Core
{
  /// <summary>
  /// Ensures all the <see cref="IDisposable"/> objects added to it are disposed
  /// on disposal of <see cref="DisposableSet"/> instance.
  /// </summary>
  /// <remarks>
  /// <note>
  /// <see cref="IDisposable.Dispose"/> methods are invoked in backward order.
  /// </note>
  /// </remarks>
  public sealed class DisposableSet: IDisposable
  {
    private HashSet<IDisposable> set;
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

    /// <summary>
    /// Joins the <see cref="DisposableSet"/> and <see cref="IDisposable"/>.
    /// </summary>
    /// <param name="first">The first disposable to join.</param>
    /// <param name="second">The second disposable to join.</param>
    /// <returns>New <see cref="JoiningDisposable"/> that will
    /// dispose both of them on its disposal</returns>
    public static JoiningDisposable operator &(DisposableSet first, IDisposable second)
    {
      return new JoiningDisposable(first, second);
    }

    private void EnsureInitialized()
    {
      if (set==null) {
        set = new HashSet<IDisposable>();
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

    /// <see cref="ClassDocTemplate.Dispose"/>
    void IDisposable.Dispose() 
    {
      try {
        if (list==null)
          return;
        using (var ea = new ExceptionAggregator()) {
          for (int i = list.Count-1; i>=0; i--)
            ea.Execute(d => d.Dispose(), list[i]);
          ea.Complete();
        }
      }
      finally {
        set = null;
        list = null;
      }
    }
  }
}