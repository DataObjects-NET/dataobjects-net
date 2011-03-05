// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class AliasProvider : TransparentProvider<Compilable.AliasProvider>
  {
    // Constructors

    public AliasProvider(Compilable.AliasProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}