// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

namespace Xtensive.Storage.Model
{
  public enum Multiplicity
  {
    OneToZero = 1,

    OneToOne = 2,

    OneToMany = 3,

    ManyToZero = 4,

    ManyToOne = 5,

    ManyToMany = 6,
  }
}