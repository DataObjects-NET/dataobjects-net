// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.06

using System;

namespace Xtensive.Core.Diagnostics
{
  [Serializable]
  public class LogImplementation : LogImplementationBase
  {
    public LogImplementation(IRealLog realLog)
      : base(realLog)
    {
    }
  }
}