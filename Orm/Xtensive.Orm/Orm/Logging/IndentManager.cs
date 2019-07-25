// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.11.11

using System;
using System.Threading;

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Log indentation manager.
  /// </summary>
  public static class IndentManager
  {
    private sealed class IndentScope : IDisposable
    {
      private readonly string oldIndent;
      private readonly Action endAction;
      private bool disposed;

      public void Dispose()
      {
        if (disposed)
          return;
        disposed = true;
        CurrentIndentValueAsync.Value = oldIndent;
        if (endAction!=null)
          endAction.Invoke();
      }

      public IndentScope(string oldIndent, Action endAction)
      {
        this.oldIndent = oldIndent;
        this.endAction = endAction;
      }
    }

    private const string SingleIndent = "  ";
    
    private static readonly AsyncLocal<string> CurrentIndentValueAsync = new AsyncLocal<string>();

    /// <summary>
    /// Gets indentation for current thread.
    /// </summary>
    public static string CurrentIdent { get { return CurrentIndentValueAsync.Value ?? string.Empty; } }

    /// <summary>
    /// Increases indentation for current thread.
    /// </summary>
    /// <returns>Indentation scope.</returns>
    public static IDisposable IncreaseIndent(Action endAction = null)
    {
      var oldIndent = CurrentIndentValueAsync.Value;
      CurrentIndentValueAsync.Value = oldIndent==null ? SingleIndent : oldIndent + SingleIndent;
      return new IndentScope(oldIndent, endAction);
    }
  }
}