// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage
{
  public partial class Session
  {
    internal EntityStateRegistry EntityStateRegistry { get; private set; }

    internal SessionCache Cache { get; private set; }

  }
}