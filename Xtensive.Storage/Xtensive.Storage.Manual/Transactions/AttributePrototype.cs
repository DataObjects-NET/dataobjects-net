// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.23

//using System;
//using System.Diagnostics;
//using System.Reflection;
//using System.Transactions;
//
//namespace Xtensive.Storage.Manual.Transactions
//{
//
//  public enum Visibility
//  {
//    Public,
//    Protected,
//    Internal,
//    Private
//  }
//
//  public enum TransactionOptions
//  {
//    ExistingOrNew,
//    New
//  }
//
//
//  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Class)]
//  public abstract class TransactionalAttributeBase : Attribute
//  {
//    public MemberTypes MembersType;
//
//    public Visibility MembersVisibility;
//
//    public bool InheritBehavior { get; set;}
//  }
//
//  public class TransactionalAttribute : TransactionalAttributeBase
//  {
//    public IsolationLevel IsolationLevel { get; set;}
//
//    public TransactionOptions Options { get; set;}
//  }
//
//  public class NotTransactionalAttribute : TransactionalAttributeBase
//  {
//  }
//
//  [NotTransactional(InheritBehavior = true)]
//  public class MyEntityBase : Entity
//  {
//  }
//
//  public class Document : Entity
//  {
//    [Transactional]
//    public void BusinessMethod()
//    {      
//    }
//
//    [Transactional(Options = TransactionOptions.New, 
//      IsolationLevel = IsolationLevel.Serializable)]
//    public void AnotherBusinessMethod()
//    {      
//    }
//  }
//
//  
//  public class MyServiceBase : SessionBound
//  { 
//  }
//
//  [Transactional(InheritBehavior = true, 
//    MembersType = MemberTypes.Method)]
//  public class DocumentService : SessionBound
//  {
//    public Document FindLastDocument()
//    {
//    }
//
//    [NotTransactional]
//    public override string ToString()
//    {
//    }
//  }
//}