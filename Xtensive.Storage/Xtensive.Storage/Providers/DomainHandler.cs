// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Providers
{
  public abstract class DomainHandler : HandlerBase
  {
    public abstract void Build();

    public Domain Domain { get; internal set; }
    public CompilationContext Compiler { get; protected set; }
  }
}