// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;

namespace Xtensive.Indexing.Storage.Model
{
  [Serializable]
  public enum ReferentialAction
  {
    None = 0,

    Default = Restrict,

    Restrict = 1,

    Cascade = 2,

    Clear = 3,
  }
}