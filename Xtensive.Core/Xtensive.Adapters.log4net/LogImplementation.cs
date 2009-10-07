// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.05

using System;
using System.Globalization;
using log4net.Util;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Adapters.log4net
{
  [Serializable]
  public sealed class LogImplementation : LogImplementationBase
  {
    protected override object GetFormattedMessage(string format, object[] args)
    {
      object stringFormat = new SystemStringFormat(CultureInfo.InvariantCulture, format, args);
      if (args == null || args.Length == 0)
        stringFormat = format;
      return stringFormat;
    }

    /// <inheritdoc/>
    public LogImplementation(IRealLog realLog)
      : base(realLog)
    {
    }
  }
}