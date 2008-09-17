// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.31

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Validation
{
  public class ValidationTest : AutoBuildTest
  {
    [HierarchyRoot(typeof (Generator), "ID")]
    public class Mouse : Entity
    {
      [Field]
      public int ID { get; set; }

      [Field]
      public int ButtonCount { get; set; }

      [Field]
      public int ScrollingCount { get; set; }

      [Field]
      public Led Led { get; set;}

      public override void OnValidate()
      {
        base.OnValidate();

        if (ButtonCount<1)
          throw new InvalidOperationException("Button count can't be less than one.");

        if (ScrollingCount > ButtonCount)
          throw new InvalidOperationException("Scrolling count can't be greater then button count.");

        if (Led!=null && Led.Brightness > 10)
          throw new InvalidOperationException("Led in the mouse is too bright.");
      }
    }

    public class Led : Structure
    {
      [Field]
      public double Brightness { get; set;}

      [Field]
      public double Precision { get; set;}

      public override void OnValidate()
      {
        if (Brightness <= 0)
          throw new InvalidOperationException("Led brightness should be greater then 0.");

        if (Precision <= 0)
          throw new InvalidOperationException("Led precision should be greater then 0.");

        if (Precision > Brightness * 10)
          throw new InvalidOperationException("Invalid precision-brightness ratio.");
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.Validation");
      return config;
    }
    
    [Test]
    public void EntityValidation()
    {
        using (Domain.OpenSession()) {
          using (Transaction.Open()) {

          // Created and modified invalid object.
          AssertEx.Throws<AggregateException>(
            delegate {
              using (Session.Current.OpenInconsistentRegion()) {
                new Mouse {ButtonCount = 2, ScrollingCount = 3};
              }
            });

          // Created, but not modified invalid object.
          AssertEx.Throws<AggregateException>(
            delegate {
              new Mouse();
            });

          // Invalid modification of existing object.
          AssertEx.Throws<AggregateException>(
            delegate {
              Mouse m;
              using (Session.Current.OpenInconsistentRegion()) {
                m = new Mouse {ButtonCount = 1, ScrollingCount = 1};
              }
              m.ScrollingCount = 2; 
            });

          Mouse mouse;

          // Valid object - ok.
          using (Session.Current.OpenInconsistentRegion()) {
            mouse = new Mouse {ButtonCount = 5, ScrollingCount = 3};
          }

          // Valid modification with invalid intermediate state - ok.
          using (Session.Current.OpenInconsistentRegion()) {
            mouse.ButtonCount = 2;
            mouse.ScrollingCount = 1;
          }

          // Invalid object is removed - ok.
          using (Session.Current.OpenInconsistentRegion()) {
            mouse.ScrollingCount = 3;
            mouse.Remove();
          }
        }
      }      
    }
    

    [Test]
    public void StructureValidation()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {

          // Valid mouse is created.
          using (Session.Current.OpenInconsistentRegion()) {
            Mouse mouse = new Mouse {ButtonCount = 2, ScrollingCount = 1};
            mouse.Led = new Led {Brightness = 7.3, Precision = 33};
          }

          // Structure become invalid.
          AssertEx.Throws<AggregateException>(
            delegate {
              Mouse mouse = new Mouse {ButtonCount = 2, ScrollingCount = 1};
              mouse.Led.Precision = 80;
            });

          // Invalid led is created.
          AssertEx.Throws<AggregateException>(
            delegate {
              new Led {Brightness = -1, Precision = 0};
            });

          // Changed structure's property makes mouse invalid.
          AssertEx.Throws<AggregateException>(
            delegate {
              Mouse mouse = new Mouse {ButtonCount = 2, ScrollingCount = 1};
              mouse.Led = new Led {Brightness = 7.3, Precision = 33};
              mouse.Led.Brightness = 11.2;
            });
        }
      }
    }

    [Test]
    public void TransactionsValidation()
    {
      using (Domain.OpenSession()) {

        // Inconsistent region can not be opened without transaction.
        AssertEx.ThrowsInvalidOperationException(
          delegate {
            Session.Current.OpenInconsistentRegion();
          });

        // Transaction can not be committed while validation context is in inconsistent state.
        AssertEx.ThrowsInvalidOperationException(
          delegate {
            using (var t = Transaction.Open()) {
              Session.Current.OpenInconsistentRegion();
              t.Complete();
            }
          });
      }
    }
  }
}
