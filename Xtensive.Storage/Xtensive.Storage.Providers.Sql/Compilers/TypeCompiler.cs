// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.29

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  /// <summary>
  /// Base class for any SQL compiler.
  /// </summary>
  /// <typeparam name="TProvider">The type of the provider.</typeparam>
  [Serializable]
  public abstract class TypeCompiler<TProvider>: Rse.Compilation.TypeCompiler<TProvider>
    where TProvider : Provider
  {
    /// <summary>
    /// Gets the <see cref="HandlerAccessor"/> object providing access to available storage handlers.
    /// </summary>
    protected HandlerAccessor Handlers { get; private set; }

    
    // Constructors
    
    /// <inheritdoc/>
    protected TypeCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
      Handlers = ((Compiler) compiler).Handlers;
    }
  }
}