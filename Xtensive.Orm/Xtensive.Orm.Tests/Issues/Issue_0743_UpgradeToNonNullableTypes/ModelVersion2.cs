// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System.Collections;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Upgrade;
using System;

namespace Xtensive.Orm.Tests.Issues.Issue_0743_UpgradeToNonNullableTypes.Model.Version2
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field(Nullable = false)]
    public string Name { get; set; }

    [Field(DefaultValue = -1)]
    public int Age { get; set; }

    [Field(Nullable = false, NullableOnUpgrade = true)]
    public Person Friend { get; set; }

    [Field(Nullable = false, DefaultValue = new byte[] {0} )]
    public Byte[] Bytes { get; set; }

    // Default definitions

    [Field(Nullable = false, DefaultValue = "A")]
    public string DefaultTest1 { get; set; }

    [Field(DefaultValue = (byte) 1)]
    public byte DefaultTest2 { get; set; }

    [Field(DefaultValue = "1900.01.01")]
    public DateTime DefaultTest3 { get; set; }

    [Field]
    public PersonData DefaultTest4 { get; set; }
  }

  public sealed class PersonData : Structure
  {
    [Field(Nullable = false, DefaultValue = "A")]
    public string DefaultTest1 { get; set; }

    [Field(DefaultValue = (byte) 1)]
    public byte DefaultTest2 { get; set; }

    [Field(DefaultValue = "1900.01.01")]
    public DateTime DefaultTest3 { get; set; }
  }
}