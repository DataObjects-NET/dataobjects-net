// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Collections
{
  /// <summary>
  /// Default <see cref="IExtensionCollection"/> implementation (<see cref="ILockable">lockable</see>).
  /// </summary>
  [Serializable]
  public class ExtensionCollection: LockableBase,
    IExtensionCollection,
    ICloneable
  {
    private Dictionary<Type, object> extensions;

    /// <inheritdoc/>
    public long Count {
      [DebuggerStepThrough]
      get {
        return extensions!=null ? extensions.Count : 0;
      }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public T Get<T>() 
      where T : class
    {
      return (T) Get(typeof (T));
    }

    /// <inheritdoc/>
    public object Get(Type extensionType)
    {
      ArgumentValidator.EnsureArgumentNotNull(extensionType, "extensionType");
      if (extensions==null)
        return null;
      object result;
      if (extensions.TryGetValue(extensionType, out result))
        return result;
      else
        return null;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public void Set<T>(T value) 
      where T : class
    {
      Set(typeof (T), value);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Wrong arguments.</exception>
    public void Set(Type extensionType, object value)
    {
      this.EnsureNotLocked();
      ArgumentValidator.EnsureArgumentNotNull(extensionType, "extensionType");
      if (extensionType.IsValueType)
        throw new ArgumentException(string.Format(
          Strings.ExTypeXMustBeReferenceType, extensionType.GetShortName()), "extensionType");
      if (value!=null && !extensionType.IsAssignableFrom(value.GetType()))
        throw new ArgumentException(string.Format(
          Strings.ExTypeXMustImplementY, value.GetType(), extensionType.GetShortName()), "value");
      
      if (extensions==null)
        if (value==null)
          return;
        else
          extensions = new Dictionary<Type, object>();
      extensions[extensionType] = value;
    }

    /// <inheritdoc/>
    public void Clear()
    {
      this.EnsureNotLocked();
      extensions = null;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (extensions!=null)
        foreach (KeyValuePair<Type, object> pair in extensions) {
          var lockable = pair.Value as ILockable;
          lockable.LockSafely(recursive);
        }
    }

    #region ICloneable methods

    /// <inheritdoc/>
    public object Clone()
    {
      return new ExtensionCollection(this);
    }

    #endregion

    #region IEnumerable methods

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<Type> GetEnumerator()
    {
      if (extensions==null)
        return EnumerableUtils<Type>.EmptyEnumerator;
      else
        return extensions.Keys.GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ExtensionCollection()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source to copy into this collection.</param>
    public ExtensionCollection(IExtensionCollection source)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (source.Count==0)
        return;
      var sourceLikeMe = source as ExtensionCollection;
      if (sourceLikeMe!=null)
        extensions = new Dictionary<Type, object>(sourceLikeMe.extensions);
      else
        foreach (Type extensionType in source)
          Set(extensionType, source.Get(extensionType));
    }
  }
}