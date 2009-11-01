// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.09

using System;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Rse.Providers
{
  public static class ProviderExtensions
  {
    public static Provider Compile(this Provider provider)
    {
      var context = CompilationScope.CurrentContext;
      if (context == null)
        throw new InvalidOperationException();
      return context.Compile(provider);
    }
  }
}