// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core.Security;
using System.Security.Cryptography;
using System.Text;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Security
{
  [TestFixture]
  public class AuthenticationServiceTest
  {
    [Test]
    public void DelegatesConstructorTest()
    {
      var userName = "aaa";
      var password1 = "aaa";
      var password2 = "aab";

      var service = new AuthenticationService((un, pas) => un==pas, (un, pas) => un + ";" + pas, 
        new HashingSignatureProvider(SHA1.Create, Encoding.UTF32));
      Assert.IsNotNull(service.Authenticate(userName, password1));
      Assert.IsNull(service.Authenticate(userName, password2));
    }

    [Test]
    public void ConstructorTest()
    {
      IAuthenticationProvider authenticationProvider = new DelegateAuthenticationProvider((un, pas) => un==pas);
      ISecurityTokenProvider securityTokenProvider = new DelegateSecurityTokenProvider((un, pas) => un + ";" + pas);
      ISignatureProvider signatureProvider = new HashingSignatureProvider(SHA1.Create, Encoding.UTF32);
      var service = new AuthenticationService(authenticationProvider, securityTokenProvider, signatureProvider);

      var userName = "aaa";
      var password1 = "aaa";
      var password2 = "aab";
      Assert.IsNotNull(service.Authenticate(userName, password1));
      Assert.IsNull(service.Authenticate(userName, password2));
    }

    private bool canFinish;

    [Test]
    public void SynchronizationTest()
    {
      var service = new AuthenticationService((un, pas) => un == pas, (un, pas) => un + ";" + pas,
        new HashingSignatureProvider(SHA1.Create, Encoding.UTF32));

      var threads = new Thread[10];
      for (int i = 0; i < 10; i++)
      {
        threads[i] = new Thread(AuthenticateTest);
      }

      try
      {
        for (int i = 0; i < 10; i++)
        {
          threads[i].Start(service);
        }
        Thread.Sleep(1000);
      }
      finally
      {
        canFinish = true;
        for (int i = 0; i < 10; i++)
        {
          threads[i].Join();
        }
      }
    }

    public void AuthenticateTest(object arg)
    {
      while (!canFinish)
      {
        var service = (AuthenticationService)arg;
        var r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        var userName = r.Next(1000).ToString();
        var password = userName;
        Assert.IsNotNull(service.Authenticate(userName, password));
      }
    }
  }
}