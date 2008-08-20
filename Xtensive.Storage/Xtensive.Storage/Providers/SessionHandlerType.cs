// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.20

using System;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Session type.
  /// </summary>
  [Flags]
  public enum SessionHandlerType
  {
    User = 0x00,
    System = 0x01,
    DomainHandler = 0x02 + System,
    Generator = 0x04 + System,
  }
}