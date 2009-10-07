// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.30

using System;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Integrity.Aspects;
using Xtensive.Integrity.Relations;
using Xtensive.Integrity.Validation;

namespace Xtensive.Integrity.Tests
{
  [Serializable]
  public class Person: AtomicBase
  {
    [Atomic]
    public string Name
    {
      get { return (string)this["Name"]; }
      [Trace(TraceOptions.All)]
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        this.Validate();
        this["Name"] = value;
      }
    }

    [Atomic]
    public int Age
    {
      get { return (int)(this["Age"] ?? 0); }
      [Trace(TraceOptions.All)]
      set {
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, 200, "value");
        this.Validate();
        this["Age"] = value;
      }
    }
    
    internal sealed class PassportRelationManager: OneToOneRelationManager<Person, Passport, PassportRelationManager> 
    {
      public static readonly bool Initialized = Initialize("Passport", "Person");
    }

    [Atomic]
    public Passport Passport
    {
      get { return (Passport)this["Passport"]; }
      [Trace(TraceOptions.All)]
      set {
        PassportRelationManager.SetMaster(this, value, (me, newValue) => {
          me["Passport"] = newValue;
        });
      }
    }

    [Changer]
    [Atomic]
    [InconsistentRegion]
    [Trace(TraceOptions.All)]
    public void SetAll(string name, int age)
    {
      Name = name;
      Age = age;
    }

    [Changer]
    [Atomic]
    [InconsistentRegion]
    [Trace(TraceOptions.All)]
    public void SetAll(string name, int age, Passport passport)
    {
      SetAll(name, age);
      Passport = passport;
    }

    [Changer]
    [Atomic]
    [InconsistentRegion]
    [Trace(TraceOptions.All)]
    public void SetAll(string name, int age, string number, string issuedBy)
    {
      SetAll(name, age);
      Passport = new Passport("0", "Unknown");
      Passport.SetAll(number, issuedBy);
    }

    public override void OnValidate()
    {
      if (Name==Age.ToString())
        throw new Exception("Name==Age.ToString()");
    }

    public bool IsCompatible(Validation.ValidationContextBase context)
    {
      ValidationContext vc = context as ValidationContext;
      return vc!=null && vc.Session==Session;
    }

    public override string ToString()
    {
      return string.Format("Person {0}, age {1}, passport {2}", Name, Age, Passport);
    }

    #region Equals implementation

    public bool Equals(Person obj)
    {
      if (obj==null) 
        return false;
      return Name==obj.Name && Age==obj.Age && Equals(Passport, obj.Passport);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj)) return true;
      return Equals(obj as Person);
    }

    #endregion


    // Constructors

    public Person(string name, int age)
    {
      Name = name;
      Age = age;
    }

    protected Person(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}