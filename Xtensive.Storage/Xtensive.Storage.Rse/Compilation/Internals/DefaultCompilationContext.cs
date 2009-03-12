// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.01

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Default <see cref="CompilationContext"/> implementation.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  public sealed class DefaultCompilationContext : CompilationContext
  {
    /// <inheritdoc/>
    public override EnumerationContext CreateEnumerationContext()
    {
      return new DefaultEnumerationContext();
    }


    // Constructors

    /// <inheritdoc/>
    public DefaultCompilationContext()
      : base(new ManagingCompiler(new DefaultCompiler()))
    {
    }
  }
}