// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.26

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Distributed.Core.Resources;

namespace Xtensive.Distributed.Core
{
  [Serializable]
  public class ElectionGroup: IEquatable<ElectionGroup>
  {
    private readonly string id;
    private readonly ReadOnlySet<NetworkEntity> participants;

    public ReadOnlySet<NetworkEntity> Participants
    {
      get { return participants; }
    }

    public string Id
    {
      get { return id; }
    }


    // Equals & GetHashCode

    public bool Equals(ElectionGroup electionGroup)
    {
      if (electionGroup == null)
        return false;
      return Equals(id, electionGroup.id);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as ElectionGroup);
    }

    public override int GetHashCode()
    {
      return id.GetHashCode();
    }


    // Constructors

    public ElectionGroup(string id, IEnumerable<NetworkEntity> participants)
    {
      if (id==null)
        throw new ArgumentNullException("id");
      if (id.Length==0)
        throw new ArgumentException(String.Format(Strings.ExArgumentCantBeEmptyString, "id"));
      this.id = id;
      this.participants = new ReadOnlySet<NetworkEntity>(new Set<NetworkEntity>(participants));
    }
  }
}