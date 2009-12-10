// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;

namespace Xtensive.Core.ObjectMapping
{
  public interface IMapper
  {
    MapperAdapter<TSource, TTarget> MapType<TSource, TSourceKey, TTarget, TTargetKey>(
      Func<TSource, TSourceKey> sourceKeyExtractor, Func<TTarget, TTargetKey> targetKeyExtractor);
  }
}