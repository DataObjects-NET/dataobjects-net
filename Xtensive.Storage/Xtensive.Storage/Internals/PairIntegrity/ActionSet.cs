// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.10

using System;

namespace Xtensive.Storage.PairIntegrity
{
  internal struct ActionSet
  {
    public Func<Entity, Entity> GetPairedValue { get; private set; }

    public Action<Entity, Entity> BreakAssociation { get; private set; }

    public Action<Entity, Entity> CreateAssociation { get; private set; }


    // Constructor

    public ActionSet(Func<Entity, Entity> getPairedValue, Action<Entity, Entity> breakAssociation, Action<Entity, Entity> createAssociation)
      : this()
    {
      GetPairedValue = getPairedValue;
      BreakAssociation = breakAssociation;
      CreateAssociation = createAssociation;
    }
  }
}