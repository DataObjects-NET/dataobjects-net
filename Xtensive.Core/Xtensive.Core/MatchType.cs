// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.12.25

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Possible types of match to look for.
  /// </summary>
  [Serializable]
  public enum MatchType
  {
    None = 0,
    Partial = 1,
    Full = 2,
  }
}
