// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.22

using System;

namespace Xtensive.Storage.Internals.Prefetch
{
  [Serializable]
  internal abstract class RootElementsPrefetcher
  {
    protected static readonly object DescriptorArraysCachingRegion = new object();
  }
}