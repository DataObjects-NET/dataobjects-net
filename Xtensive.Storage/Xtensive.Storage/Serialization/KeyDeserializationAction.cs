// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.24

using System;

namespace Xtensive.Storage.Serialization
{
  [Serializable]
  public enum KeyDeserializationAction
  {
    Keep = 0,
    Regenerate = 1
  }
}