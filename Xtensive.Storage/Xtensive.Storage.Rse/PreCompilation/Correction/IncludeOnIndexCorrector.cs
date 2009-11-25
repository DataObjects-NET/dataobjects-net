// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.25

using System;
using System.Diagnostics;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  /// <summary>
  /// Corrects include provider on index provider. Rewrites it to set of seeks.
  /// </summary>
  [Serializable]
  public sealed class IncludeOnIndexCorrector : IPreCompiler
  {
    public CompilableProvider Process(CompilableProvider rootProvider)
    {
      return new IncludeOnIndexRewriter().VisitCompilable(rootProvider);
    }
  }
}