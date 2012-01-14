// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.20

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Tests.Reflection
{

  #region GetTypeName test model

  public class A<T1, T2>
  {
    public class B<T3>
    {
    }

    public T1 F1;
    public T2 F2;

    public A()
    {
    }

    public A(T1 f1, T2 f2)
    {
      F1 = f1;
      F2 = f2;
    }
  }

  #endregion

  #region CreateAssociate test model

  // Interfaces

  public interface IAssociate
  {
  }

  public interface I
  {
  }

  public interface IT<T>
  {
  }

  public interface IT2<T1, T2>
  {
  }

  // Interface associates

  public class InterfaceAssociate: IAssociate
  {
  }

  public class TInterfaceAssociate<T>: IAssociate
  {
  }

  public class T2InterfaceAssociate<T1, T2>: IAssociate
  {
  }

  // Class 1

  public class A
  {
  }

  public class AI: A, I
  {
  }

  public class AIT<T>: A, IT<T>
  {
  }

  public class AIT2<T1, T2>: A, IT2<T1, T2>
  {
  }

  // Class 1 associates (all direct)

  public class AAssociate: IAssociate
  {
  }

  public class AIAssociate: IAssociate
  {
  }

  public class AITAssociate<T>: IAssociate
  {
  }

  public class AIT2Associate<T1, T2>: IAssociate
  {
  }

  // Class 2

  public class BA: A
  {
  }

  public class BI: I
  {
  }

  public class BIT<T>: IT<T>
  {
  }

  public class BIT2<T1, T2>: IT2<T1, T2>
  {
  }

  // No class 2 associates (all indirect, => to A & I associates)

  // Class 3

  public class CB: BA
  {
  }

  public class CBI: BI
  {
  }

  public class CBIT<T>: BIT<T>
  {
  }

  public class CBIT2<T1, T2>: BIT2<T1, T2>
  {
  }

  // No class 3 associates (all indirect, => to A & I associates)

  // Class 4

  public class DA: A
  {
  }

  public class DAI: AI
  {
  }

  public class DAIT<T>: AIT<T>
  {
  }

  public class DAIT2<T1, T2>: AIT2<T1, T2>
  {
  }

  // No class 4 associates (all indirect, => to A associates)

  // Class 5

  public class EA: A
  {
  }

  public class EAI: AI
  {
  }

  public class EAIT<T>: AIT<T>
  {
  }

  public class EAIT2<T1, T2>: AIT2<T1, T2>
  {
  }

  // Class 5 associates (all direct)

  public class EAAssociate: IAssociate
  {
  }

  public class EAIAssociate: IAssociate
  {
  }

  public class EAITAssociate<T>: IAssociate
  {
  }

  public class EAIT2Associate<T1, T2>: IAssociate
  {
  }

  // Wrong classes

  public class FIIT<T>: I, IT<T>
  {
  }

  public class GFIIT<T>: FIIT<T>
  {
  }

  public class GIT2FIIT<T1, T2>: FIIT<T1>, IT2<T1, T2>
  {
  }

  // Associates to base types, common interface
  
  public class SomeEnumerable : 
    IEnumerable
  {
    public IEnumerator GetEnumerator()
    {
      throw new NotImplementedException();
    }
  }

  public class SomeCloneableEnumerable : SomeEnumerable, 
    ICloneable
  {
    public object Clone()
    {
      throw new NotImplementedException();
    }
  }

  public interface ITestHandler
  {
    object DoSomethingWithValue(object value);
  }

  public interface ITestHandler<T>: ITestHandler
  {
    T DoSomethingWithValue(T value);
  }

  public class TestHandlerBase: ITestHandler
  {
    public object DoSomethingWithValue(object value)
    {
      return value;
    }
  }

  public class ObjectTestHandler : TestHandlerBase, 
    ITestHandler<object>
  {
  }

  public class ObjectTestHandler<T> : TestHandlerBase, 
    ITestHandler<T>
  {
    public T DoSomethingWithValue(T value)
    {
      return value;
    }
  }

  public class Int32TestHandler : TestHandlerBase, 
    ITestHandler<int>
  {
    public int DoSomethingWithValue(int value)
    {
      return value;
    }
  }

  public class Int64TestHandler : TestHandlerBase, 
    ITestHandler<long>
  {
    public long DoSomethingWithValue(long value)
    {
      return value;
    }
  }

  public class ArrayTestHandler<T> : TestHandlerBase, 
    ITestHandler<T[]>
  {
    public T[] DoSomethingWithValue(T[] value)
    {
      return value;
    }
  }

  public class Array2DTestHandler<T> : TestHandlerBase, 
    ITestHandler<T[,]>
  {
    public T[,] DoSomethingWithValue(T[,] value)
    {
      return value;
    }
  }

  public class EnumerableInterfaceTestHandler : TestHandlerBase, 
    ITestHandler<IEnumerable>
  {
    public IEnumerable DoSomethingWithValue(IEnumerable value)
    {
      return value;
    }
  }

  public class EnumerableInterfaceTestHandler<T> : TestHandlerBase, 
    ITestHandler<IEnumerable<T>>
  {
    public IEnumerable<T> DoSomethingWithValue(IEnumerable<T> value)
    {
      return value;
    }
  }

  public class CloneableInterfaceTestHandler : TestHandlerBase, 
    ITestHandler<ICloneable>
  {
    public ICloneable DoSomethingWithValue(ICloneable value)
    {
      return value;
    }
  }

  #endregion
}