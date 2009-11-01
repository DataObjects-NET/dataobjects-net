// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Xtensive.Core
{
  /// <summary>
  /// Describes an object that has <see cref="Name"/> property.
  /// </summary>
  public interface IHasName
  {
    /// <summary>
    /// Gets or sets the name of the object.
    /// </summary>
    string Name { get; set; }
  }
}
