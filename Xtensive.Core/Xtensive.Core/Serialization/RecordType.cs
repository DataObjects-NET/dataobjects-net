// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.28

namespace Xtensive.Core.Serialization
{
  public enum RecordType : byte
  {
    Default = Unknown,

    Unknown = 0,

    Descriptor = 1,

    Object = 2,

    Reference = 3,
  }
}