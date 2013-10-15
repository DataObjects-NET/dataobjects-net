// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Log, which writes nothing.
  /// </summary>
  public sealed class NullLog : BaseLog
  {
    /// <inheritdoc/>
    public override IDisposable DebugRegion(string message, params object[] parameters)
    {
      return null;
    }

    /// <inheritdoc/>
    public override void Debug(string message, object[] parameters, Exception exception = null)
    {
    }

    /// <inheritdoc/>
    public override void Info(string message, object[] parameters, Exception exception = null)
    {
    }

    /// <inheritdoc/>
    public override IDisposable InfoRegion(string message, params object[] parameters)
    {
      return null;
    }

    /// <inheritdoc/>
    public override void Warn(string message, object[] parameters, Exception exception = null)
    {
    }

    /// <inheritdoc/>
    public override void Error(string message, object[] parameters, Exception exception = null)
    {
    }

    /// <inheritdoc/>
    public override void Fatal(string message, object[] parameters, Exception exception = null)
    {
    }
  }
}