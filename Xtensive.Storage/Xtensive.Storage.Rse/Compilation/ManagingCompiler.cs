// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse.Compilation
{
  [Serializable]
  public sealed class ManagingCompiler : CompilerBase
  {
    private readonly Dictionary<UrlInfo, ICompiler> compilersMap;

    /// <inheritdoc/>
    public override UrlInfo Location
    {
      get { throw new NotSupportedException(); }
    }

    /// <inheritdoc/>
    public override bool IsCompatible(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override ExecutableProvider Compile(CompilableProvider cp)
    {
      if (cp == null)
        return null;
      ExecutableProvider result;
      ProviderType providerType = cp.Type;
      switch (providerType) {
        case ProviderType.Transfer:
          result = CompileTransfer((TransferProvider)cp);
          break;
        case ProviderType.Apply:
          result = CompileApply((ApplyProvider)cp);
          break;
        default:
          var unary = cp as UnaryProvider;
          if (unary != null)
            result = CompileUnary(unary);
          else {
            var binary = cp as BinaryProvider;
            if (binary != null)
              result = CompileBinary(binary);
            else {
              var lac = (LocationAwareProvider)cp;
              var location = lac.Location;
              var compiler = compilersMap[location];
              result = compiler.Compile(lac);
            }
          }
          break;
      }
      return result;
    }

    private ExecutableProvider CompileBinary(BinaryProvider binary)
    {
      var left = Compile(binary.Left);
      var right = Compile(binary.Right);

      var compiler = left.Location == RseCompiler.DefaultClientLocation 
        ? compilersMap[right.Location]
        : compilersMap[left.Location];

      using (CompiledSources.Add(binary.Left, left) & CompiledSources.Add(binary.Right, right))
        return compiler.Compile(binary);
    }

    private ExecutableProvider CompileUnary(UnaryProvider unaryProvider)
    {
      var source = Compile(unaryProvider.Source);
      var compiler = compilersMap[source.Location];
      using (CompiledSources.Add(unaryProvider.Source, source))
        return compiler.Compile(unaryProvider);
    }

    private ExecutableProvider CompileTransfer(TransferProvider provider)
    {
      return Compile(provider.Source);
    }

    private ExecutableProvider CompileApply(ApplyProvider provider)
    {
      var left = Compile(provider.Left);
      using (CompiledSources.Add(provider.ApplyParameter, left)) {
        var right = Compile(provider.Right);
        left = CompiledSources[provider.ApplyParameter];
        var compiler = compilersMap[right.Location];
        using (CompiledSources.Add(provider.Left, left) & CompiledSources.Add(provider.Right, right))
          return compiler.Compile(provider);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ManagingCompiler(BindingCollection<object, ExecutableProvider> compiledSources, params ICompiler[] compilers)
      : this(compiledSources, (IEnumerable<ICompiler>)compilers)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ManagingCompiler(BindingCollection<object, ExecutableProvider> compiledSources, IEnumerable<ICompiler> compilers)
      : base(compiledSources)
    {
      compilersMap = compilers.ToDictionary(ic => ic.Location);
      var firstServerCompiler = compilers.Where(ic => ic.Location != RseCompiler.DefaultClientLocation).FirstOrDefault();
      if (firstServerCompiler != null && !compilersMap.ContainsKey(RseCompiler.DefaultServerLocation))
        compilersMap.Add(RseCompiler.DefaultServerLocation, firstServerCompiler);
    }
  }
}