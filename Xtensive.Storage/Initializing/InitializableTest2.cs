// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.05


using System;
using Xtensive.Core.Aspects;
using Xtensive.Storage.Initializing.Initializable1;

namespace Xtensive.Storage.Initializing.Initializable2
{
  [Serializable]
  [Initializable]
  public sealed class SortProvider : CompilableProvider
  {
    public bool IsInitialized { get; set; }

    protected override void Initialize()
    {
      IsInitialized = true;
    }

    // Constructors

    public SortProvider(int value, object arg)
      : base (value)
    {
      IsInitialized = false;
    }
  }  
}
