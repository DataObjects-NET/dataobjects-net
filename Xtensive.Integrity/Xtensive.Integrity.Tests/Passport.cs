// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.30

using System;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Integrity.Aspects;

namespace Xtensive.Integrity.Tests
{
  [Serializable]
  public class Passport: AtomicBase, 
    IEquatable<Passport>
  {
    [Atomic]
    public Person Person
    {
      get { return (Person)this["Person"]; }
      [Trace(TraceOptions.All)]
      set {
        Person.PassportRelationManager.SetSlave(this, value, (me, newValue) => {
          me["Person"] = newValue;
        });
      }
    }

    [Atomic]
    public string Number
    {
      get { return (string)this["Number"]; }
      [Trace(TraceOptions.All)]
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        int.Parse(value);
        this["Number"] = value;
      }
    }

    [Atomic]
    public string IssuedBy
    {
      get { return (string)this["IssuedBy"]; }
      [Trace(TraceOptions.All)]
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        this["IssuedBy"] = value;
      }
    }
    
    [Changer]
    [Atomic]
    [Trace(TraceOptions.All)]
    public void SetAll(string number, string issuedBy)
    {
      Number = number;
      IssuedBy = issuedBy;
    }

    public override string ToString()
    {
      return string.Format("Passport #{0} from {1}", Number, IssuedBy);
    }

    #region Equality members

    public override int GetHashCode()
    {
      return unchecked (
        Number.GetHashCode() * 329 + 
        IssuedBy.GetHashCode()
        );
    }

    public bool Equals(Passport obj)
    {
      if (obj==null) 
        return false;
      return Number==obj.Number && IssuedBy==obj.IssuedBy;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj)) return true;
      return Equals(obj as Passport);
    }

    #endregion


    // Constructors

    public Passport(string number, string issuedBy)
    {
      Number = number;
      IssuedBy = issuedBy;
    }

    protected Passport(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}