// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.01.26

namespace Xtensive.Storage.Linq
{
  public enum MemberType
  {
    Default = Unknown,
    Primitive = 0,
    Key = 1,
    Structure = 2,
    Entity = 3,
    EntitySet = 4,
    NonPersistent = 5,
    Unknown = 6,
  }
}