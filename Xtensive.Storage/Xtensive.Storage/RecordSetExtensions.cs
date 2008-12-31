// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System;
using System.Collections.Generic;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  /// <summary>
  /// <see cref="RecordSet"/> related extension methods.
  /// </summary>
  public static class RecordSetExtensions
  {
    /// <summary>
    /// Converts the <see cref="RecordSet"/> items to <see cref="Entity"/> instances.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Entity"/> instances to get.</typeparam>
    /// <param name="source">The <see cref="RecordSet"/> to process.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public static IEnumerable<T> ToEntities<T>(this RecordSet source) 
      where T : class, IEntity
    {
      foreach (var entity in ToEntities(source, typeof (T)))
        yield return entity as T;
    }

    /// <summary>
    /// Converts the <see cref="RecordSet"/> items to <see cref="Entity"/> instances.
    /// </summary>
    /// <param name="source">The <see cref="RecordSet"/> to process.</param>
    /// <param name="type">The type of <see cref="Entity"/> instances to get.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public static IEnumerable<Entity> ToEntities(this RecordSet source, Type type)
    {

//      ArgumentValidator.EnsureArgumentNotNull(source, "source");
//
//      var context = new RecordSetParserContext(source);
//      var recordSetMapping = GetMapping(source.Header);
//      var groupMappings = recordSetMapping.Mappings;
//      var groupMappingCount = groupMappings.Count;
//      if (groupMappingCount == 0)
//        yield break;
//      var typeMappings = new TypeMapping[groupMappingCount];
//
//      foreach (var tuple in source)
//      {
//        Entity entity = null;
//        for (int i = 0; i < groupMappingCount; i++)
//        {
//          Key key = Parse(context, tuple, groupMappings[i], ref typeMappings[i]);
//          if (entity == null && key != null && type.IsAssignableFrom(key.Type.UnderlyingType))
//            entity = key.Resolve();
//        }
//        yield return entity;
//      }

      return Domain.Current.RecordSetParser.ToEntities(source, type);
    }

    public static IEnumerable<Record> Parse(this RecordSet source)
    {
      return Domain.Current.RecordSetParser.Parse(source);
    }

    public static Record ParseFirst(this RecordSet source)
    {
      return Domain.Current.RecordSetParser.ParseFirst(source);
    }
  }
}
