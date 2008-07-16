// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.16

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// The context of <see cref="ExecutableProvider"/> evaluation.
  /// </summary>
  public sealed class ExecutionContext: Context<ExecutionScope>
  {
    private Dictionary<Pair<object, string>, object> cache = new Dictionary<Pair<object, string>, object>();

    /// <summary>
    /// Gets the current execution context.
    /// The same as <see cref="ExecutionScope.CurrentContext"/>.
    /// </summary>
    public static ExecutionContext Current {
      get {
        return ExecutionScope.CurrentContext;
      }
    }

    /// <summary>
    /// Gets the current execution context.
    /// Fails, if it is <see langword="null"/> and <paramref name="failIfNone"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="failIfNone">Indicates whether to throw an exception, if there is no context.</param>
    /// <exception cref="InvalidOperationException"><see cref="Current"/> is <see langword="null"/> and 
    /// <paramref name="failIfNone"/> is <see langword="true"/>.</exception>
    public static ExecutionContext GetCurrent(bool failIfNone)
    {
      var c = Current;
      if (failIfNone && c==null)
        throw new InvalidOperationException(Strings.ExActiveExecutionContextIsNecessary);
      return c;
    }

    #region IContext<...> methods

    /// <inheritdoc/>
    protected override ExecutionScope CreateActiveScope()
    {
      return new ExecutionScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive {
      get {
        return ExecutionScope.CurrentContext==this;
      }
    }

    #endregion

    #region Internal methods: SetCachedValue, GetCachedValue

    internal void SetCachedValue<T>(Pair<object, string> key, T value)
      where T: class
    {
      cache[key] = value;
    }

    internal T GetCachedValue<T>(Pair<object, string> key)
      where T: class
    {
      object result;
      if (cache.TryGetValue(key, out result))
        return result as T;
      else
        return null;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ExecutionContext()
    {
    }
  }
}