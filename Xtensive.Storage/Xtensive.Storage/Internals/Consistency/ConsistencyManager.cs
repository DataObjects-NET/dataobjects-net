// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.18

using System.Linq;
using Xtensive.Integrity;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals.Consistency
{
  internal static class ConsistencyManager
  {
    public static void CheckConstraints(Entity target)
    {
      CheckNotNullConstraints(target);
    }

    private static void CheckNotNullConstraints(Entity target)
    {
      TypeInfo type = target.Type;
      IndexInfo index = type.Indexes.PrimaryIndex;
      int i = 0;
      foreach (ColumnInfo column in index.Columns.Where(c => !c.IsNullable)) {
        if (!target.Tuple.IsAvailable(i) || target.Tuple.IsNull(i))
          throw new ConstraintViolationException(string.Format("Entity '{0}': Column '{1}' cannot have null value.", target.Key, column));
        i++;
      }
    }
  }
}