// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.09.25

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Distributed.Core
{
  [Serializable]
  public class NetworkEntity: IEquatable<NetworkEntity>
  {
    private readonly string id;
    private readonly ReadOnlyDictionary<string, string> endPointUrls;

    public string Id
    {
      get { return id; }
    }

    public ReadOnlyDictionary<string, string> EndPointUrls
    {
      get { return endPointUrls; }
    }

    
    // Equals & GetHashCode

    public bool Equals(NetworkEntity networkEntity)
    {
      if (null == (object)networkEntity)
        return false;
      return Equals(id, networkEntity.id);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as NetworkEntity);
    }

    public override int GetHashCode()
    {
      return id.GetHashCode();
    }

    public static bool operator ==(NetworkEntity n1, NetworkEntity n2)
    {
      if ((null == (object)n1) && (null == (object)n2))
        return true;
      if (null == (object)n1)
        return false;
      return n1.Equals(n2);
    }

    public static bool operator !=(NetworkEntity n1, NetworkEntity n2)
    {
      return !(n1 == n2);
    }


    // Constructors

    public NetworkEntity(string id, IDictionary<string, string> endPointUrls)
    {
      this.id = id;
      if (endPointUrls is ReadOnlyDictionary<string,string>)
        this.endPointUrls = (ReadOnlyDictionary<string, string>)endPointUrls;
      else
        this.endPointUrls = new ReadOnlyDictionary<string, string>(endPointUrls);
    }
  }
}