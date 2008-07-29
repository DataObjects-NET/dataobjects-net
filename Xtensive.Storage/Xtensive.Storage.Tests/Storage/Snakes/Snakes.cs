// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.16

using System.Diagnostics;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.SnakeModel
{
  public enum Features
  {
    None = 0,
    CanCrawl = 1,
    CanJump = 2,
    CanFly = 3,
    CanWalk = 4,
  }

  [DebuggerDisplay("Name = '{Name}'")]
  [Index("Name")]
  [HierarchyRoot(typeof (DefaultGenerator), "ID")]
  public class Creature : Entity
  {
    [Field]
    public int ID { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Features Features { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Length = {Length}")]
  public class Snake : Creature
  {
    [Field]
    public int Length { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Color = {Color}")]
  public class Lizard : Creature
  {
    [Field]
    public string Color { get; set; }
  }
}