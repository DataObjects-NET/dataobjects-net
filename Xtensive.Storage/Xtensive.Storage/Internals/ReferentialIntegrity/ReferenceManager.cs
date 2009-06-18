// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class ReferenceManager : SessionBound
  {
    private static readonly CascadeProcessor CascadeProcessor = new CascadeProcessor();
    private static readonly RestrictProcessor DenyProcessor = new RestrictProcessor();
    private static readonly ClearProcessor ClearProcessor = new ClearProcessor();

    public RemovalContext Context { get; internal set; }

    public void ClearReferencesTo(Entity referencedObject, bool notify)
    {
      if (Context!=null) {
        ClearReferencesTo(Context, referencedObject);
        return;
      }

      using (Context = new RemovalContext(this, notify))
        ClearReferencesTo(Context, referencedObject);
    }

    private void ClearReferencesTo(RemovalContext context, Entity referencedObject)
    {
      context.RemovalQueue.Add(referencedObject.State);
      ApplyAction(context, referencedObject, OnRemoveAction.Deny);
      ApplyAction(context, referencedObject, OnRemoveAction.Clear);
      ApplyAction(context, referencedObject, OnRemoveAction.Cascade);
    }

    public void ApplyAction(RemovalContext context, Entity referencedObject, OnRemoveAction action)
    {
      List<AssociationInfo> associations = referencedObject.Type.GetAssociations().Where(a => a.OnRemove==action).ToList();
      if (associations.Count==0)
        return;

      ActionProcessor processor = GetProcessor(action);
      foreach (AssociationInfo association in associations)
        foreach (Entity referencingObject in association.FindReferencingObjects(referencedObject))
          processor.Process(context, association, referencingObject, referencedObject);
    }

    public ActionProcessor GetProcessor(OnRemoveAction action)
    {
      switch (action) {
        case OnRemoveAction.Clear:
          return ClearProcessor;
        case OnRemoveAction.Default:
          return DenyProcessor;
        case OnRemoveAction.Cascade:
          return CascadeProcessor;
        default:
          throw new ArgumentOutOfRangeException("action");
      }
    }


    // Constructors

    public ReferenceManager(Session session)
      : base(session)
    {
    }
  }
}