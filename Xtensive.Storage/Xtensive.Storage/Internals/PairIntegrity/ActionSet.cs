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
    public Func<IEntity, bool, IEntity> GetPairedValue { get; private set; }

    public Action<IEntity, IEntity, bool> BreakAssociation { get; private set; }

    public Action<IEntity, IEntity, bool> CreateAssociation { get; private set; }


    // Constructors

    public ActionSet(Func<IEntity, bool, IEntity> getPairedValue, Action<IEntity, IEntity, bool> breakAssociation, Action<IEntity, IEntity, bool> createAssociation)
      : this()
    {
      GetPairedValue = getPairedValue;
      BreakAssociation = breakAssociation;
      CreateAssociation = createAssociation;
    }
  }
}