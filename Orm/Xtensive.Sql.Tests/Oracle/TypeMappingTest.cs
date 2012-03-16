// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.31

using NUnit.Framework;

namespace Xtensive.Sql.Tests.Oracle
{
  public abstract class TypeMappingTest : Tests.TypeMappingTest
  {
    protected override void CheckEquality(object expected, object actual)
    {
      var arrayValue = expected as byte[];
      var stringValue = expected as string;
      var charValue = expected as char?;

      bool nullExpected = 
        arrayValue!=null && arrayValue.Length==0 ||
        stringValue!=null && stringValue.Length==0 ||
        charValue!=null && charValue==default(char);
      
      if (nullExpected)
        Assert.IsNull(actual);
      else
        base.CheckEquality(expected, actual);
    }

  }
}