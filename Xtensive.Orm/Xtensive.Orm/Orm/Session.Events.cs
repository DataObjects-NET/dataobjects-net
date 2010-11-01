// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.29

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Transactions;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm
{
  public partial class Session
  {
    /// <summary>
    /// Gets access point to all <see cref="Session"/>-related events.
    /// </summary>
    public SessionEventAccessor Events { get; private set; }

    /// <summary>
    /// Gets access point to all system <see cref="Session"/>-related events.
    /// </summary>
    public SessionEventAccessor SystemEvents { get; private set; }

    /// <summary>
    /// The manager of <see cref="Entity"/>'s events.
    /// </summary>
    internal EntityEventBroker EntityEvents { get; private set; }

    /// <summary>
    /// Raises events on all <see cref="INotifyPropertyChanged"/> and
    /// <see cref="INotifyCollectionChanged"/> subscribers stating that
    /// all entities and collections are changed.
    /// </summary>
    public void NotifyChanged()
    {
      NotifyChanged(
        NotifyChangedOptions.SkipRemovedEntities | 
        NotifyChangedOptions.Prefetch);
    }

    /// <summary>
    /// Raises events on all <see cref="INotifyPropertyChanged"/> and
    /// <see cref="INotifyCollectionChanged"/> subscribers stating that
    /// all entities and collections are changed.
    /// </summary>
    /// <param name="options">The options.</param>
    public void NotifyChanged(NotifyChangedOptions options)
    {
      using (Activate()) 
      using (var tx = OpenAutoTransaction()) {
        var entitySubscribers    = EntityEvents.GetSubscribers(EntityEventBroker.PropertyChangedEventKey).ToList();
        var entitySetSubscribers = EntityEvents.GetSubscribers(EntityEventBroker.CollectionChangedEventKey).ToList();

        if ((options & NotifyChangedOptions.Prefetch)==NotifyChangedOptions.Prefetch) {
          var keys =
            from triplet in entitySubscribers
            select triplet.First;
          keys.Prefetch(this).Run();
        }

        var skipRemovedEntities = 
          (options & NotifyChangedOptions.SkipRemovedEntities)==NotifyChangedOptions.SkipRemovedEntities;
        foreach (var triplet in entitySubscribers) {
          if (triplet.Third!=null) {
            var handler = (PropertyChangedEventHandler) triplet.Third;
            var key = triplet.First;
            var entityState = EntityStateCache[key, false];
            var sender = entityState!=null ? entityState.Entity : Query.SingleOrDefault(key);
            if (skipRemovedEntities && sender.IsRemoved())
              continue;
            handler.Invoke(sender, new PropertyChangedEventArgs(null));
          }
        }

        foreach (var triplet in entitySetSubscribers) {
          if (triplet.Third!=null) {
            var handler = (NotifyCollectionChangedEventHandler) triplet.Third;
            var key = triplet.First;
            var entityState = EntityStateCache[key, false];
            var owner = entityState!=null ? entityState.Entity : Query.SingleOrDefault(key);
            var ownerIsRemoved = owner.IsRemoved();
            if (skipRemovedEntities && ownerIsRemoved)
              continue;
            var sender = ownerIsRemoved ? null : owner.GetFieldValue(triplet.Second);
            handler.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
          }
        }
        tx.Complete();
      }
    }
  }
}