// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.09

using System.Transactions;
using Xtensive.Collections;

namespace Xtensive.Orm
{
  partial class Session
  {
    /// <summary>
    /// Single access point allowing to run LINQ queries,
    /// create future (delayed) and compiled queries,
    /// and finally, resolve <see cref="Key"/>s to <see cref="Entity">entities</see>.
    /// </summary>
    public QueryEndpoint Query { get; private set; }
  }
}
