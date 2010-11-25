// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.27

using System;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm.Manual.Validation
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    [LengthConstraint(Min = 2, Max = 128)]
    [NotNullOrEmptyConstraint]
    public string FirstName { get; set;} 

    [Field]
    [LengthConstraint(Min = 2, Max = 128)]
    [NotNullOrEmptyConstraint]
    public string LastName { get; set;} 
    
    [Field]
    [PastConstraint(Mode = ConstrainMode.OnSetValue)]
    public DateTime BirthDay { get; set; } 

    [Field]
    public bool IsSubscribedOnNews { get; set;}
    
    [Field]
    [EmailConstraint]
    public string Email { get; set;}

    [PhoneNumberConstraint]
    public string Phone { get; set;}

    [Field]
    [RangeConstraint(Min = 0.8, Max = 2.23,
      Message = "Incorrect '{PropertyName}' value: {value}, it can not be less than {Min} and greater than {Max}.")]
    public double Height { get; set;}

    protected override void OnValidate()
    {
      base.OnValidate();
      if (IsSubscribedOnNews && string.IsNullOrEmpty(Email))
        throw new Exception("Can not subscribe on news (email is not specified).");
    }

    public Person(Session session)
      : base(session)
    {}
  }
}