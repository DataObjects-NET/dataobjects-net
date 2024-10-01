// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.17

using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Configuration of log. 
  /// </summary>
  public class LogConfiguration : LockableBase
  {
    private string source;
    private string target;

    /// <summary>
    /// Gets or sets source or sources of log separated by comma.
    /// </summary>
    public string Source {
      get => source;
      set {
        EnsureNotLocked();
        source = value;
      }
    }

    /// <summary>
    /// Gets or sets targer of log.
    /// </summary>
    public string Target {
      get => target;
      set {
        EnsureNotLocked();
        target = value;
      }
    }

    /// <summary>
    /// Creates new instance of this class
    /// </summary>
    /// <param name="source">Source or sources for new log. Sources must be separated by comma</param>
    /// <param name="target">Targer for new log</param>
    public LogConfiguration(string source, string target)
    {
      this.source = source;
      this.target = target;
    }
  }
}
