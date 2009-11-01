// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.20

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing.Composite;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Index factory. Creates and configures indexes.
  /// </summary>
  public static class IndexFactory
  {
    /// <summary>
    /// Creates the unique index.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TImplementation">The type of the index implementation.</typeparam>
    /// <param name="configuration">The index descriptor.</param>
    /// <returns>Newly created and initialized with <paramref name="configuration"/> 
    /// <see cref="IUniqueIndex{TKey,TItem}"/> instance</returns>
    public static IUniqueIndex<TKey, TItem> CreateUnique<TKey, TItem, TImplementation>(IndexConfigurationBase<TKey, TItem> configuration) 
      where TImplementation: IUniqueIndex<TKey, TItem>, IConfigurable<IndexConfigurationBase<TKey, TItem>>, new()
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      TImplementation index = new TImplementation();
      index.Configure(configuration);
      return index;
    }

    /// <summary>
    /// Creates the unique index.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TImplementation">The type of the index implementation.</typeparam>
    /// <param name="configuration">The index descriptor.</param>
    /// <returns>Newly created and initialized with <paramref name="configuration"/> 
    /// <see cref="IUniqueIndex{TKey,TItem}"/> instance</returns>
    public static IUniqueOrderedIndex<TKey, TItem> CreateUniqueOrdered<TKey, TItem, TImplementation>(IndexConfigurationBase<TKey, TItem> configuration) 
      where TImplementation: IUniqueOrderedIndex<TKey, TItem>, IConfigurable<IndexConfigurationBase<TKey, TItem>>, new()
    {
      return (IUniqueOrderedIndex<TKey, TItem>)CreateUnique<TKey, TItem, TImplementation>(configuration);
    }

    /// <summary>
    /// Creates the non-unique index.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TUniqueKey">The type of the unique key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TImplementation">The type of the underlying index implementation.</typeparam>
    /// <param name="configuration">The index configuration.</param>
    /// <returns>
    /// Newly created and initialized with <paramref name="configuration"/>
    /// <see cref="INonUniqueIndex{TKey,TItem}"/> instance.
    /// </returns>
    public static INonUniqueIndex<TKey, TItem> CreateNonUnique<TKey, TUniqueKey, TItem, TImplementation>(NonUniqueIndexConfiguration<TKey, TUniqueKey, TItem> configuration) 
      where TImplementation: IUniqueOrderedIndex<TUniqueKey, TItem>, IConfigurable<IndexConfigurationBase<TUniqueKey, TItem>>, new()
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      configuration.Validate();

      if (configuration.IsLocked)
        configuration = (NonUniqueIndexConfiguration<TKey, TUniqueKey, TItem>)configuration.Clone();

      TImplementation uniqueIndex = new TImplementation();
      for (int i = 0; i<configuration.Measures.Count; i++) {
        IMeasure<TItem> measure = configuration.Measures[i];
        if (configuration.UniqueIndexConfiguration.Measures.GetItem<IMeasure<TItem>>(measure.Name) == null)
          configuration.UniqueIndexConfiguration.Measures.Add(measure);
      }
      uniqueIndex.Configure(configuration.UniqueIndexConfiguration);
      configuration.UniqueIndex = uniqueIndex;

      NonUniqueIndex<TKey, TUniqueKey, TItem> nonUniqueKey = new NonUniqueIndex<TKey, TUniqueKey, TItem>();
      nonUniqueKey.Configure(configuration);
      return nonUniqueKey;
    }

    /// <summary>
    /// Creates the composite index.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TImplementation">The type of the underlying unique index implementation.</typeparam>
    /// <param name="configuration">The index configuration.</param>
    /// <returns>
    /// Newly created and initialized with <paramref name="configuration"/>
    /// <see cref="CompositeIndex{TKey,TItem}"/> instance.
    /// </returns>
    public static CompositeIndex<TKey, TItem> CreateComposite<TKey, TItem, TImplementation>(Composite.IndexConfiguration<TKey, TItem> configuration)
      where TKey : Tuple
      where TItem : Tuple
      where TImplementation: IUniqueOrderedIndex<TKey, TItem>, IConfigurable<IndexConfigurationBase<TKey, TItem>>, new()
      
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      if (configuration.IsLocked)
        configuration = (Composite.IndexConfiguration<TKey, TItem>)configuration.Clone();

      Composite.IndexConfiguration<TKey, TItem> compositeConfig = configuration;
      IndexConfigurationBase<TKey,TItem> implConfig = configuration.UniqueIndexConfiguration;

      // Shared measures
      if (compositeConfig.Measures[CountMeasure<object, long>.CommonName] == null)
        compositeConfig.Measures.Add(new CountMeasure<TItem, long>());

      long count = compositeConfig.Measures.Count;
      for (int i = 0; i < count; i++)
      {
        IMeasure<TItem> measure = compositeConfig.Measures[i];
        if (implConfig.Measures[measure.Name] == null)
          implConfig.Measures.Add(measure);
      }

      // Segment configuration & measures
      int segmentNumber = 0;
      foreach (IndexSegmentConfiguration<TKey, TItem> segmentConfig in compositeConfig.Segments)
      {
        segmentConfig.SegmentNumber = segmentNumber++;
        segmentConfig.KeyExtractor = compositeConfig.KeyExtractor;
        segmentConfig.KeyComparer = compositeConfig.KeyComparer;

        if (segmentConfig.Measures[CountMeasure<object, long>.CommonName] == null)
          segmentConfig.Measures.Add(new CountMeasure<TItem, long>());

        count = compositeConfig.Measures.Count;
        for (int i = 0; i < count; i++)
        {
          IMeasure<TItem> segmentMeasure = segmentConfig.Measures[i];
          string implMeasureName = segmentConfig.SegmentName + segmentMeasure.Name;
          //implConfig.Measures.Add(segmentMeasure);`
          segmentConfig.MeasureMapping[segmentMeasure.Name] = implMeasureName;
        }
      }

      if (implConfig.KeyExtractor == null)
        implConfig.KeyExtractor = delegate(TItem compositeItem)
        {
          CutInTransform<int> keyTransform = new CutInTransform<int>(false, compositeConfig.KeyExtractor(compositeItem).Count, compositeConfig.KeyExtractor(compositeItem).Descriptor, (int)compositeItem[compositeConfig.KeyExtractor(compositeItem).Count]);
          return (TKey) keyTransform.Apply(TupleTransformType.TransformedTuple, compositeConfig.KeyExtractor(compositeItem), (int) compositeItem[compositeConfig.KeyExtractor(compositeItem).Count]);
            
        };
      if (implConfig.KeyComparer == null)
        implConfig.KeyComparer = compositeConfig.KeyComparer.Provider.GetComparer<TKey>();
      implConfig.EntireKeyComparer =
        compositeConfig.KeyComparer.Provider.GetComparer<IEntire<TKey>>().Cast<IEntire<TKey>>();

      TImplementation uniqueIndex = new TImplementation();
      uniqueIndex.Configure(implConfig);
      compositeConfig.UniqueIndex = uniqueIndex;

      CompositeIndex<TKey, TItem> index = new CompositeIndex<TKey, TItem>();
      index.Configure(compositeConfig);
      return index;
    }
  }
}