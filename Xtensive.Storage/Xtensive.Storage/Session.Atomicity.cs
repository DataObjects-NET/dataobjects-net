// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using Xtensive.Storage;

namespace Xtensive.Storage
{
  public partial class Session 
  {
    /// <summary>
    /// Gets the atomicity context.
    /// </summary>
    public AtomicityContext AtomicityContext { get; private set; }
  }
}