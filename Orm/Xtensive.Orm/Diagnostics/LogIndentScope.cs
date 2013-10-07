// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using Xtensive.Core;

using Xtensive.IoC;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Log indent scope. 
  /// An instance of this class adds indent to all logged messages
  /// in the current thread. Dispose the instance to remove the indent.
  /// </summary>
  public sealed class LogIndentScope: Scope<LogIndent>
  {
    internal new static LogIndentScope CurrentScope {
      get {
        return (LogIndentScope)Scope<LogIndent>.CurrentScope;
      }
    }

    private new LogIndentScope OuterScope
    {
      get { return (LogIndentScope)base.OuterScope; }
    }

    /// <summary>
    /// Gets current indent value.
    /// </summary>
    public static int CurrentIndent {
      get {
        LogIndentScope currentScope = CurrentScope;
        return currentScope==null ? 0 : currentScope.Indent;
      }
    }

    /// <summary>
    /// Gets current indent as string.
    /// </summary>
    public static string CurrentIndentString {
      get {
        LogIndentScope currentScope = CurrentScope;
        return currentScope==null ? string.Empty : currentScope.IndentString;
      }
    }

    /// <summary>
    /// Gets the indent associated with this scope.
    /// </summary>
    public int Indent
    {
      get { return Context.Value; }
    }

    /// <summary>
    /// Gets the indent associated with this scope as string.
    /// </summary>
    public string IndentString
    {
      get { return Context.ToString(); }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public LogIndentScope()
      : base(LogIndent.Create(CurrentIndent+1))
    {
    }
  }
}