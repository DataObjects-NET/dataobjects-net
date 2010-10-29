// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.22

using System;
using System.Collections.Generic;
using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping.Comparison
{
  internal sealed class ComparisonInfo
  {
    #region Nested classes

    private struct Memento 
    {
      public TargetPropertyDescription TargetProperty;

      public object Owner;

      public TargetPropertyDescription[] StructurePath;


      // Constructors

      public Memento(Memento memento)
      {
        TargetProperty = memento.TargetProperty;
        StructurePath = memento.StructurePath;
        Owner = memento.Owner;
      }
    }

    private class Caretaker : IDisposable
    {
      private readonly Stack<Memento> states = new Stack<Memento>(32);
      private readonly ComparisonInfo originator;

      public void SaveMemento()
      {
        states.Push(originator.currentMemento);
        originator.currentMemento = new Memento(originator.currentMemento);
      }


      // Constructors

      public Caretaker(ComparisonInfo originator)
      {
        this.originator = originator;
      }

      public void Dispose()
      {
        originator.currentMemento = states.Pop();
      }
    }

    #endregion

    private Memento currentMemento;
    private readonly Caretaker caretaker;

    public TargetPropertyDescription TargetProperty
    {
      get { return currentMemento.TargetProperty; }
      set { currentMemento.TargetProperty = value; }
    }

    public object Owner
    {
      get { return currentMemento.Owner; }
      set { currentMemento.Owner = value; }
    }

    public TargetPropertyDescription[] StructurePath
    {
      get { return currentMemento.StructurePath; }
      set { currentMemento.StructurePath = value; }
    }

    public IDisposable SaveState()
    {
      caretaker.SaveMemento();
      return caretaker;
    }


    // Constructors

    public ComparisonInfo()
    {
      caretaker = new Caretaker(this);
    }
  }
}