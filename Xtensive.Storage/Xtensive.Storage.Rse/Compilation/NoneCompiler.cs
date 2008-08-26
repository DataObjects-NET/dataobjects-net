// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.26

using System;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class NoneCompiler : ICompiler
  {
    public bool IsCompatible(ExecutableProvider provider)
    {
      return true;
    }

    public ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }

    public ICompiler FallbackCompiler
    {
      get { return null; }
    }

    public ExecutableProvider Compile(CompilableProvider provider)
    {
      return null;
    }
  }
}