// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// This interface should be supported by any indentifier
  /// that can provide the type of object it belongs to.
  /// </summary>
  public interface ITypedIdentifier
  {
    Type IdentifiedType { get; }
  }
}