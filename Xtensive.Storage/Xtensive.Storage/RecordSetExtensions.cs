// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System;
using System.Collections.Generic;
using Xtensive.Storage.Rse;
using System.Linq;

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
      var parser = Domain.Current.RecordSetParser;
      foreach (var record in parser.Parse(source)) {
        Entity entity = null;
        foreach (var key in record.PrimaryKeys) {
          if (entity == null && key != null && type.IsAssignableFrom(key.Type.UnderlyingType))
            entity = key.Resolve();
        }
        yield return entity;
      }


//      var mapping = parser.GetMapping(source.Header);
//      var typeInfo = Domain.Current.Model.Types[type];
//      var matchedGroup = mapping.Mappings.Select((gm,i) => new {gm,i}).FirstOrDefault(p => p.gm.GetMapping(typeInfo.TypeId) != null);
//      if (matchedGroup == null)
//        throw new InvalidOperationException(string.Format("Could not resolve into entities of type '{0}'", type));
//      int groupIndex = matchedGroup.i;
//
//      foreach (var record in parser.Parse(source)) {
//        var key = record[groupIndex];
//        if (key != null)
//          yield return key.Resolve();
//        yield return null;
//      }
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
