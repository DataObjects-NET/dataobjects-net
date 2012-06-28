// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Caching
{
  /// <summary>
  /// Abstract base class for storing cached values.
  /// </summary>
  /// <typeparam name="TValue">The type of <see cref="Value"/>.</typeparam>
  [DebuggerDisplay("{State.First}, (IsActual: {IsActual}")]
  public abstract class CachedValueBase<TValue, TActualizationInfo> : 
    ICachedValue<TValue>
  {
    /// <summary>
    /// Gets the value; 
    /// if the <see cref="State"/> isn't actual (see <see cref="IsActual"/>),
    /// the value will be recalculated by <see cref="Refresh"/> method.
    /// </summary>
    public TValue Value {
      get {
        if (!IsActual) {
          var syncRoot = SyncRoot;
          if (syncRoot!=null) {
            lock (syncRoot)
              Refresh();
          }
          else
            Refresh();
        }
        return State.First;
      }
    }

    /// <summary>
    /// Gets the underlying state of this object.
    /// </summary>
    protected Pair<TValue, TActualizationInfo> State { get; set; }

    /// <inheritdoc/>
    public object SyncRoot { get; private set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is actual,
    /// so an attempt to read it will not lead to invocation of
    /// <see cref="Refresh"/> method.
    /// </summary>
    public abstract bool IsActual { get; }

    /// <summary>
    /// Invalidates the <see cref="State"/> by setting its value
    /// to <c>default(Pair&lt;TValue, TActualizationInfo&gt;)</c>.
    /// </summary>
    public void Invalidate()
    {
      State = default(Pair<TValue, TActualizationInfo>);
    }

    /// <summary>
    /// Refreshes the <see cref="State"/> when <see cref="Value"/>
    /// is requested, but state isn't actual (see <see cref="IsActual"/>).
    /// </summary>
    protected abstract void Refresh();

    /// <inheritdoc/>
    public override string ToString()
    {
      return Value.ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected CachedValueBase()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="syncRoot">The object to synchronize for duration of
    /// <see cref="Refresh"/> method call; 
    /// <see langword="null" /> means no synchronization is implied.</param>
    protected CachedValueBase(object syncRoot)
    {
      SyncRoot = syncRoot;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="syncOnItself">If set to <see langword="true"/>, this instance
    /// will synchronize on itself while invoking <see cref="Refresh"/> method;
    /// otherwise it won't synchronize at all.</param>
    protected CachedValueBase(bool syncOnItself)
    {
      if (syncOnItself)
        SyncRoot = this;
    }
  }
}