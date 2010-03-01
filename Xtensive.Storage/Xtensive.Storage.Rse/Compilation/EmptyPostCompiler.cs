// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.12

using System;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Empty <see cref="IPostCompiler"/> implementation.
  /// </summary>
  [Serializable]
  public sealed class EmptyPostCompiler : IPostCompiler
  {
    public ExecutableProvider Process(ExecutableProvider rootProvider)
    {
      return rootProvider;
    }
  }
}