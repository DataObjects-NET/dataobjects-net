using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Disconnected
{
  internal class DisconnectedEntitySetState : ICloneable
  {
    public Key OwnerKey { get; private set; }

    public string FieldName { get; private set; }

    public HashSet<Key> Items { get; private set; }

    public HashSet<Key> RemovedItems { get; private set; }

    public bool IsFullyLoaded { get; set; }

    /// <inheritdoc/>
    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Creates a new <see cref="DisconnectedEntitySetState"/> instance 
    /// that is a copy of the current instance.
    /// </summary>
    /// <returns>A new <see cref="DisconnectedEntitySetState"/> instance 
    /// that is a copy of this instance.</returns>
    public DisconnectedEntitySetState Clone()
    {
      var newState = new DisconnectedEntitySetState(OwnerKey, FieldName);
      newState.IsFullyLoaded = IsFullyLoaded;
      foreach (var item in Items)
        newState.Items.Add(item);
      foreach (var item in RemovedItems)
        newState.RemovedItems.Add(item);
      return newState;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisconnectedEntitySetState(Key ownerKey, string fieldName)
    {
      ArgumentValidator.EnsureArgumentNotNull(ownerKey, "ownerKey");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fieldName, "fieldName");

      OwnerKey = ownerKey;
      FieldName = fieldName;
      Items = new HashSet<Key>();
      RemovedItems = new HashSet<Key>();
    }
  }
}