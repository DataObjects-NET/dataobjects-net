// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using System;

namespace Xtensive.Core.Aspects
{
  [Flags]
  public enum TraceOptions
  {
    Default   = 0x11,
    Enter     = 0x1,
    Exit      = 0x2,
    Arguments = 0x4,
    Result    = 0x8,
    Indent    = 0x10,
    All       = 0x1F,
  }
}