// Copyright (C) 2013-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    public struct IndentScope : IDisposable
    {
      private readonly int oldIndent;
      private readonly Action endAction;
      private bool disposed;

      public void Dispose()
      {
        if (disposed)
          return;
        disposed = true;
        CurrentIndentValueAsync.Value = oldIndent;
        endAction?.Invoke();
      }

      public IndentScope(int oldIndent, Action endAction)
      {
        this.oldIndent = oldIndent;
        this.endAction = endAction;
        disposed = false;
      }
    }

    private const int SingleIndent = 2;
    
    private static readonly AsyncLocal<int> CurrentIndentValueAsync = new();

    /// <summary>
    /// Gets indentation for current thread.
    /// </summary>
    public static int CurrentIdent => CurrentIndentValueAsync.Value;

    /// <summary>
    /// Increases indentation for current thread.
    /// </summary>
    /// <returns>Indentation scope.</returns>
    public static IndentScope IncreaseIndent(Action endAction = null)
    {
      var oldIndent = CurrentIndentValueAsync.Value;
      CurrentIndentValueAsync.Value = oldIndent + SingleIndent;
      return new IndentScope(oldIndent, endAction);
    }
  }
}
