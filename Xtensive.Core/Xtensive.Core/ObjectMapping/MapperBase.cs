// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  public abstract class MapperBase : IMapper
  {
    private readonly GraphTransformer transformer;
    private readonly GraphComparer comparer;
    internal readonly MappingInfo mappingInfo;

    /// <inheritdoc/>
    public MapperAdapter<TSource, TTarget> MapType<TSource, TSourceKey, TTarget, TTargetKey>(
      Func<TSource, TSourceKey> sourceKeyExtractor, Func<TTarget, TTargetKey> targetKeyExtractor)
    {
      mappingInfo.Register(typeof (TSource), source => sourceKeyExtractor.Invoke((TSource) source),
        typeof (TTarget), target => targetKeyExtractor.Invoke((TTarget) target));
      return new MapperAdapter<TSource, TTarget>(this);
    }

    public object Transform(object source)
    {
      if (!mappingInfo.IsLocked)
        throw new InvalidOperationException(Strings.ExMappingConfigurationHasNotBeenCompleted);
      return transformer.Transform(source);
    }

    public void Compare(object originalTarget, object modifiedTarget)
    {
      comparer.Compare(originalTarget, modifiedTarget);
    }

    protected abstract void OnObjectModified(ModificationDescriptor descriptor);

    internal void Complete()
    {
      mappingInfo.Build();
    }


    // Constructors

    protected MapperBase()
    {
      mappingInfo = new MappingInfo();
      transformer = new GraphTransformer(mappingInfo);
      comparer = new GraphComparer(mappingInfo, OnObjectModified, new DefaultExistanceInfoProvider());
    }
  }
}