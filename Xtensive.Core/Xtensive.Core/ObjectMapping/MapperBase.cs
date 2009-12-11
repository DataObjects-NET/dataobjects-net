// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  public abstract class MapperBase : IMapper
  {
    private GraphTransformer transformer;
    private GraphComparer comparer;

    public MappingDescription MappingDescription { get; private set; }

    internal MappingBuilder MappingBuilder { get; private set; }

    /// <inheritdoc/>
    public MapperAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Func<TTarget, TKey> targetKeyExtractor)
    {
      MappingBuilder.Register(typeof (TSource), source => sourceKeyExtractor.Invoke((TSource) source),
        typeof (TTarget), target => targetKeyExtractor.Invoke((TTarget) target));

      return new MapperAdapter<TSource, TTarget>(this);
    }

    public object Transform(object source)
    {
      if (MappingDescription == null)
        throw new InvalidOperationException(Strings.ExMappingConfigurationHasNotBeenCompleted);
      return transformer.Transform(source);
    }

    public void Compare(object originalTarget, object modifiedTarget)
    {
      if (MappingDescription == null)
        throw new InvalidOperationException(Strings.ExMappingConfigurationHasNotBeenCompleted);
      comparer.Compare(originalTarget, modifiedTarget);
    }

    protected abstract void OnObjectModified(ModificationDescriptor descriptor);

    internal void Complete()
    {
      MappingDescription = MappingBuilder.Build();
      MappingBuilder = null;
      transformer = new GraphTransformer(MappingDescription);
      comparer = new GraphComparer(MappingDescription, OnObjectModified, new DefaultExistanceInfoProvider());
    }


    // Constructors

    protected MapperBase()
    {
      MappingBuilder = new MappingBuilder();
    }
  }
}