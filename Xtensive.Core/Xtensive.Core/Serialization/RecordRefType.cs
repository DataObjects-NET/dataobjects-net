// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.04.03

namespace Xtensive.Core.Serialization
{
  public enum RecordRefType : byte
  {
    None = 0,

    Extern = 1,

    Self = 2,

    Parent = 3,
  }
}