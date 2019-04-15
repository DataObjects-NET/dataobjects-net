// Copyright (C) 2003-2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.10.12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm
{
  /// <summary>
  /// Entity remove reason
  /// </summary>
  public enum EntityRemoveReason
  {
    /// <summary>
    /// Remove caused by other reasons
    /// </summary>
    Other = 0,

    /// <summary>
    /// Remove caused by user code
    /// </summary>
    User = 1,

    /// <summary>
    /// Remove caused by association
    /// </summary>
    Association = 2
  }
}
