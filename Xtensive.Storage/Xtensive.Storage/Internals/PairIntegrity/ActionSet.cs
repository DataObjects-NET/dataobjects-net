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
    public Func<Entity, bool,  Entity> GetPairedValue { get; private set; }

    public Action<Entity, Entity, bool> BreakAssociation { get; private set; }

    public Action<Entity, Entity, bool> CreateAssociation { get; private set; }


    // Constructor

    public ActionSet(Func<Entity, bool, Entity> getPairedValue, Action<Entity, Entity, bool> breakAssociation, Action<Entity, Entity, bool> createAssociation)
      : this()
    {
      GetPairedValue = getPairedValue;
      BreakAssociation = breakAssociation;
      CreateAssociation = createAssociation;
    }
  }
}