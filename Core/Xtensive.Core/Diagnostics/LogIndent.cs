// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Resources;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Used internally by <see cref="LogIndentScope"/>.
  /// </summary>
  [Serializable]
  public class LogIndent: IEquatable<LogIndent>, 
    IContext<LogIndentScope>
  {
    private static Dictionary<int, LogIndent> indents = new Dictionary<int, LogIndent>();
    private readonly int value;
    private readonly string stringValue;

    public int Value
    {
      get { return value; }
    }

    public string StringValue
    {
      get { return stringValue; }
    }

    public bool Equals(LogIndent logIndent)
    {
      return value == logIndent.value;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is LogIndent))
        return false;
      return Equals((LogIndent)obj);
    }

    public override int GetHashCode()
    {
      return value;
    }

    public override string ToString()
    {
      return stringValue;
    }

    public static LogIndent Create(int indent)
    {
      LogIndent logIndent = null;
      if (indents.TryGetValue(indent, out logIndent))
        return logIndent;
      Dictionary<int, LogIndent> newIndents = new Dictionary<int, LogIndent>(indents);
      logIndent = new LogIndent(indent);
      newIndents[indent] = logIndent;
      indents = newIndents;
      return logIndent;
    }

    #region IContext<LogIndentScope> Members

    /// <inheritdoc/>
    public bool IsActive {
      get {
        return LogIndentScope.CurrentIndent==Value;
      }
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public LogIndentScope Activate()
    {
      throw new NotSupportedException(Strings.ExUseLogIndentScopeConstructorInstead);
    }

    #endregion


    // Constructors

    private LogIndent(int value)
    {
      this.value = value;
      stringValue = string.Empty;
      for (int i = 0; i<value; i++)
        stringValue += "  ";
      stringValue = String.Intern(stringValue);
    }
  }
}