// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.12.18

using System.Collections.Generic;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals
{
  internal class CommandInfo
  {
    internal Command Command { get; set; }

    internal List<SqlLoadTask> SelectTasks { get; set; }

    internal int ReentersCount { get; set; }
  }
}
