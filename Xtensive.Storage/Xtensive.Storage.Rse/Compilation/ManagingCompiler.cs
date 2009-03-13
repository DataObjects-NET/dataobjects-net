// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse.Compilation
{
  [Serializable]
  public sealed class ManagingCompiler : ICompiler
  {
    private readonly Dictionary<UrlInfo, ICompiler> compilersMap;
    private Func<object, ExecutableProvider, Binding> Bind 
    { 
      get
      {
        return CompilationContext.Current.BindingContext.Bind;
      }
    }

    /// <inheritdoc/>
    public UrlInfo Location
    {
      get { throw new NotSupportedException(); }
    }

    bool ICompiler.IsCompatible(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }

    ExecutableProvider ICompiler.ToCompatible(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }

    public ExecutableProvider Compile(CompilableProvider cp)
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

      using (Bind(binary.Left, left) & Bind(binary.Right, right))
        return compiler.Compile(binary);
    }

    private ExecutableProvider CompileUnary(UnaryProvider unaryProvider)
    {
      var source = Compile(unaryProvider.Source);
      var compiler = compilersMap[source.Location];
      using (Bind(unaryProvider.Source, source))
        return compiler.Compile(unaryProvider);
    }

    private ExecutableProvider CompileTransfer(TransferProvider provider)
    {
      return Compile(provider.Source);
    }

    private ExecutableProvider CompileApply(ApplyProvider provider)
    {
      var left = Compile(provider.Left);
      var right = Compile(provider.Right);

      var compiler = left.Location == RseCompiler.DefaultClientLocation
        ? compilersMap[right.Location]
        : compilersMap[left.Location];

      using (Bind(provider.Left, left) & Bind(provider.Right, right))
        return compiler.Compile(provider);
    }

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ManagingCompiler(params ICompiler[] compilers)
      : this((IEnumerable<ICompiler>)compilers)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ManagingCompiler(IEnumerable<ICompiler> compilers)
    {
      compilersMap = compilers.ToDictionary(ic => ic.Location);
      var firstServerCompiler = compilers.Where(ic => ic.Location != RseCompiler.DefaultClientLocation).FirstOrDefault();
      if (firstServerCompiler != null && !compilersMap.ContainsKey(RseCompiler.DefaultServerLocation))
        compilersMap.Add(RseCompiler.DefaultServerLocation, firstServerCompiler);
    }
  }
}