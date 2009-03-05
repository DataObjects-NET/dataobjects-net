// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.05

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  [Serializable]
  public sealed class PredicateJoin : BinaryProvider
  {


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>  
    public PredicateJoin(CompilableProvider left, CompilableProvider right)
      : base(ProviderType.PredicateJoin, left, right)
    {}
  }
}