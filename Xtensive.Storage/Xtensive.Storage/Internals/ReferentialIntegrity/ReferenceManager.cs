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
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class ReferenceManager : SessionBound
  {
    private static readonly CascadeProcessor cascadeProcessor = new CascadeProcessor();
    private static readonly RestrictProcessor restrictProcessor = new RestrictProcessor();
    private static readonly ClearProcessor clearProcessor = new ClearProcessor();

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

    private static void ClearReferencesTo(RemovalContext context, Entity referencedObject)
    {
      context.RemovalQueue.Add(referencedObject.State);
      ApplyAction(context, referencedObject, ReferentialAction.Restrict);
      ApplyAction(context, referencedObject, ReferentialAction.Clear);
      ApplyAction(context, referencedObject, ReferentialAction.Cascade);
    }

    private static void ApplyAction(RemovalContext context, Entity referencedObject, ReferentialAction action)
    {
      IEnumerable<AssociationInfo> associations = referencedObject.Type.GetAssociations().Where(a => a.OnRemove==action);
      if (associations == null)
        return;

      ActionProcessor processor = GetProcessor(action);
      foreach (AssociationInfo association in associations) {
        var referencingObjects = AssociationBrowser.FindReferencingObjects(association, referencedObject);
        if (referencingObjects == null)
          continue;
        foreach (Entity referencingObject in referencingObjects)
          if (!context.RemovalQueue.Contains(referencingObject.State))
            processor.Process(context, association, referencingObject, referencedObject);
      }
    }

    private static ActionProcessor GetProcessor(ReferentialAction action)
    {
      switch (action) {
      case ReferentialAction.Clear:
        return clearProcessor;
      case ReferentialAction.Default:
        return restrictProcessor;
      case ReferentialAction.Cascade:
        return cascadeProcessor;
      default:
        throw new ArgumentOutOfRangeException("action");
      }
    }


    // Constructor

    public ReferenceManager(Session session)
      : base(session)
    {
    }
  }
}