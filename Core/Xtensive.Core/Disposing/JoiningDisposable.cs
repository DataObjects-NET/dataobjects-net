// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.27

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Disposing
{
  /// <summary>
  /// Disposes two <see cref="IDisposable"/> objects.
  /// </summary>
  [Serializable]
  public sealed class JoiningDisposable : IDisposable
  {
    private IDisposable first;
    private IDisposable second;

    /// <summary>
    /// Gets the first object to dispose.
    /// </summary>
    public IDisposable First {
      get { return first; }
    }

    /// <summary>
    /// Gets the second object to dispose.
    /// </summary>
    public IDisposable Second {
      get { return second; }
    }

    /// <summary>
    /// Joins the <see cref="JoiningDisposable"/> and <see cref="IDisposable"/>.
    /// </summary>
    /// <param name="first">The first disposable to join.</param>
    /// <param name="second">The second disposable to join.</param>
    /// <returns>New <see cref="JoiningDisposable"/> that will
    /// dispose both of them on its disposal</returns>
    public static JoiningDisposable operator &(JoiningDisposable first, IDisposable second)
    {
      if (second==null)
        return first;
      return new JoiningDisposable(first, second);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="disposable1">The first disposable.</param>
    /// <param name="disposable2">The second disposable.</param>
    public JoiningDisposable(IDisposable disposable1, IDisposable disposable2)
    {
      this.first = disposable1;
      this.second = disposable2;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      var d1 = first;
      first = null;
      try {
        d1.DisposeSafely();
      }
      catch (Exception ex) {
        using (var ea = new ExceptionAggregator()) {
          ea.Execute(_this => {
            var d2 = _this.second;
            _this.second = null;
            d2.DisposeSafely();
          }, this);
          ea.Execute(e => { throw e; }, ex);
          ea.Complete();
        }
      }
      d1 = second;
      second = null;
      d1.DisposeSafely();
    }
  }
}