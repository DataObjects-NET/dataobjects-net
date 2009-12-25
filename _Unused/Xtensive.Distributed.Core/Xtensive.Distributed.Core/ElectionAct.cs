// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.09.25

using System;
using Xtensive.Distributed.Core.Resources;

namespace Xtensive.Distributed.Core
{
  [Serializable]
  public class ElectionAct: IEquatable<ElectionAct>
  {
    private readonly Guid id;
    private ElectionResult result;

    public Guid Id
    {
      get { return id; }
    }

    public ElectionResult Result
    {
      get { return result; }
    }

    internal void SetResult(ElectionResult result)
    {
      if (this.result != null)
        throw new ArgumentException(Strings.ExSpecifiedElectionActIsAlreadyAssociatedWithResult);
      this.result = result;
    }

    
    // Equals & GetHashCode

    public bool Equals(ElectionAct electionAct)
    {
      if (null == (object)electionAct)
        return false;
      return Equals(id, electionAct.id);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as ElectionAct);
    }

    public override int GetHashCode()
    {
      return id.GetHashCode();
    }

    public static bool operator ==(ElectionAct a1, ElectionAct a2)
    {
      if ((null == (object)a1) && (null == (object)a2))
        return true;
      if (null == (object)a1)
        return false;
      return a1.Equals(a2);
    }

    public static bool operator !=(ElectionAct a1, ElectionAct a2)
    {
      return !(a1 == a2);
    }

    public ElectionAct Clone()
    {
      if (null != result)
        throw new NotImplementedException();
      return new ElectionAct(this.id);
    }


    // Constructors

    internal ElectionAct(Guid id)
    {
      this.id = id;
    }
  }
}