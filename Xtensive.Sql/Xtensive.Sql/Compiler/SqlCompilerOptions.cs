// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Compiler
{
  [Flags]
  public enum SqlCompilerOptions
  {
    /// <summary>
    /// None of options are active
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Default set of options.
    /// </summary>
    Default = ForcedAliasing,

    /// <summary>
    /// Should forced (full automatic) aliasing be applied during compilation?
    /// </summary>
    ForcedAliasing = 0x1,
  }
}
