// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Providers.MsSql.Compilers
{
  public sealed class Compiler : Sql.Compilers.Compiler
  {
    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Compiler(HandlerAccessor handlers)
      : base(handlers)
    {}
  }
}