// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.11

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  public sealed class SitePreferenceCompiler : ICompiler
  {
    /// <summary>
    /// Gets the server side compiler.
    /// </summary>
    public ICompiler ServerCompiler { get; private set; }

    /// <summary>
    /// Gets the client side compiler.
    /// </summary>
    public ICompiler ClientCompiler { get; private set; }

    /// <inheritdoc/>
    public ExecutableProvider Compile(CompilableProvider provider, ExecutableProvider[] sources)
    {
      if (provider == null)
        return null;
      if (sources.Any(s => s == null))
        return null;

      var executionSiteProvider = provider as ExecutionSiteProvider;
      if (executionSiteProvider!=null)
        return sources[0];
      if (provider.Sources.Length == 1) {
        var siteProvider = provider.Sources[0] as ExecutionSiteProvider;
        if (siteProvider != null) {
          if (siteProvider.Options == ExecutionOptions.Client)
            return ClientCompiler.Compile(provider, sources);
          return ServerCompiler.Compile(provider, sources);
        }
      }
      return ServerCompiler.Compile(provider, sources);
    }

    bool ICompiler.IsCompatible(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }

    ExecutableProvider ICompiler.ToCompatible(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }


    // Constructor

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serverCompiler">The server compiler.</param>
    /// <param name="clientCompiler">The client compiler.</param>
    public SitePreferenceCompiler(ICompiler serverCompiler, ICompiler clientCompiler)
    {
      ServerCompiler = serverCompiler;
      ClientCompiler = clientCompiler;
    }
  }
}