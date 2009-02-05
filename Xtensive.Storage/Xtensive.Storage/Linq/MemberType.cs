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
    Unknown = 0,
    Field,
    Key,
    Structure,
    Entity,
    EntitySet
  }
}