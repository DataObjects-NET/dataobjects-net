// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Model.Stored.Internals
{
  internal sealed class TypeMappingUpdater
  {
    public void UpdateMappings(StoredDomainModel model, NodeConfiguration nodeConfiguration)
    {
      ArgumentNullException.ThrowIfNull(model);
      ArgumentNullException.ThrowIfNull(nodeConfiguration);

      foreach (var storedType in model.Types) {
        if (!storedType.MappingDatabase.IsNullOrEmpty())
          storedType.MappingDatabase = nodeConfiguration.DatabaseMapping.Apply(storedType.MappingDatabase);
        if (!storedType.MappingSchema.IsNullOrEmpty())
          storedType.MappingSchema = nodeConfiguration.SchemaMapping.Apply(storedType.MappingSchema);
      }
    }
  }
}
