// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.31

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Validation
{ 
  public class ValidationTest : AutoBuildTest
  {
    [ThreadStatic]
    private static int validationCallsCount;

    [HierarchyRoot]
    public class Mouse : Entity
    {
      [Field, Key]
      public int ID { get; private set; }
        
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

        if (Led.Brightness > 10)
          throw new InvalidOperationException("Led in the mouse is too bright.");

        validationCallsCount++;
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
        base.OnValidate();

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
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Mouse).Namespace);
      return config;
    }

    [Test]
    public void ValidationCallsCountTest()
    { 
      validationCallsCount = 0;
      int mouseId;

      using (Domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          using (Session.Current.OpenInconsistentRegion()) {
            Mouse mouse = new Mouse();
            mouse.ButtonCount = 2;
            mouse.ScrollingCount = 1;
            mouse.Led.Brightness = 1.5;
            mouse.Led.Precision = 1.5;

            mouseId = mouse.ID;
          }          
          transactionScope.Complete();
        }
      }
      Assert.AreEqual(1, validationCallsCount);

      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          Mouse mouse = Query<Mouse>.All.Where(m => m.ID==mouseId).First();
        }
      }
      Assert.AreEqual(1, validationCallsCount); 
    }
    
    [Test]
    public void EntityValidation()
    {
        using (Domain.OpenSession()) {
          using (Transaction.Open()) {

          // Created and modified invalid object. (ScrollingCount > ButtonCount)
          AssertEx.Throws<AggregateException>(
            delegate {
              using (Session.Current.OpenInconsistentRegion()) {
                new Mouse {ButtonCount = 2, ScrollingCount = 3, Led = new Led { Brightness = 1, Precision = 1 }};
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
                m = new Mouse {ButtonCount = 1, ScrollingCount = 1, Led = new Led { Brightness = 1, Precision = 1 }};
              }
              m.ScrollingCount = 2; 
            });

          Mouse mouse;

          // Valid object - ok.
          using (Session.Current.OpenInconsistentRegion()) {
            mouse = new Mouse {ButtonCount = 5, ScrollingCount = 3};
            mouse.Led.Precision = 1;
            mouse.Led.Brightness = 2;
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
        Mouse mouse;

        using (var transactionScope = Transaction.Open()) {

          // Valid mouse is created.
          using (Session.Current.OpenInconsistentRegion()) {
            mouse = new Mouse {ButtonCount = 2, ScrollingCount = 1};
            mouse.Led = new Led {Brightness = 7.3, Precision = 33};
          }
          transactionScope.Complete();
        }

        // Structure become invalid.
        using (var transactionScope = Transaction.Open()) {
          AssertEx.Throws<AggregateException>(
            delegate {
              // structurebug workaround
              mouse.ButtonCount = mouse.ButtonCount;

              mouse.Led.Brightness = 2.3;
              transactionScope.Complete();
            });
        }
          
        // Changed structure make entity invalid.
        using (var transactionScope = Transaction.Open()) {
          AssertEx.Throws<AggregateException>(
            delegate {
              mouse.Led.Brightness = 11;
              transactionScope.Complete();
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
