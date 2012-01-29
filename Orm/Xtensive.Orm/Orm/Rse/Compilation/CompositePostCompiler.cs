// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.29

using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Compilation
{
  public class CompositePostCompiler : IPostCompiler
  {
    private readonly IPostCompiler[] postCompilers;

    public ExecutableProvider Process(ExecutableProvider rootProvider)
    {
      var provider = rootProvider;
      foreach (var optimizer in postCompilers)
        provider = optimizer.Process(provider);
      return provider;
    }


    // Constructors

    public CompositePostCompiler(params IPostCompiler[] postCompilers)
    {
      this.postCompilers = postCompilers;
    }
  }
}