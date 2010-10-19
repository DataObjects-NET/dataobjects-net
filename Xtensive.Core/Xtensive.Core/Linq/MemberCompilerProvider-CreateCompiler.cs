// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.10

using System;
using System.Reflection;
using Xtensive.Reflection;

namespace Xtensive.Linq
{
  partial class MemberCompilerProvider<T>
  {
    private static Func<T, T[], T> CreateInvokerForInstanceCompiler(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
      case 1:
        var d1 = DelegateHelper.CreateDelegate<Func<T, T>>(null, t, s);
        return (_this, args) => d1(_this);
      case 2:
        var d2 = DelegateHelper.CreateDelegate<Func<T, T, T>>(null, t, s);
        return (_this, args) => d2(_this, args[0]);
      case 3:
        var d3 = DelegateHelper.CreateDelegate<Func<T, T, T, T>>(null, t, s);
        return (_this, args) => d3(_this, args[0], args[1]);
      case 4:
        var d4 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T>>(null, t, s);
        return (_this, args) => d4(_this, args[0], args[1], args[2]);
      case 5:
        var d5 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d5(_this, args[0], args[1], args[2], args[3]);
      case 6:
        var d6 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d6(_this, args[0], args[1], args[2], args[3], args[4]);
      case 7:
        var d7 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d7(_this, args[0], args[1], args[2], args[3], args[4], args[5]);
      case 8:
        var d8 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d8(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
      case 9:
        var d9 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d9(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
      case 10:
        var d10 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d10(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
      case 11:
        var d11 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d11(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
      case 12:
        var d12 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d12(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
      case 13:
        var d13 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d13(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
      case 14:
        var d14 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d14(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
      case 15:
        var d15 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d15(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
      case 16:
        var d16 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_this, args) => d16(_this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);
      }
      return null;
    }
    
    private static Func<MemberInfo, T, T[], T> CreateInvokerForInstanceGenericCompiler(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
      case 2:
        var d2 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T>>(null, t, s);
        return (member, _this, args) => d2(member, _this);
      case 3:
        var d3 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T>>(null, t, s);
        return (member, _this, args) => d3(member, _this, args[0]);
      case 4:
        var d4 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d4(member, _this, args[0], args[1]);
      case 5:
        var d5 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d5(member, _this, args[0], args[1], args[2]);
      case 6:
        var d6 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d6(member, _this, args[0], args[1], args[2], args[3]);
      case 7:
        var d7 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d7(member, _this, args[0], args[1], args[2], args[3], args[4]);
      case 8:
        var d8 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d8(member, _this, args[0], args[1], args[2], args[3], args[4], args[5]);
      case 9:
        var d9 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d9(member, _this, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
      case 10:
        var d10 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d10(member, _this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
      case 11:
        var d11 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d11(member, _this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
      case 12:
        var d12 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d12(member, _this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
      case 13:
        var d13 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d13(member, _this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
      case 14:
        var d14 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d14(member, _this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
      case 15:
        var d15 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d15(member, _this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
      case 16:
        var d16 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _this, args) => d16(member, _this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
      }    
      return null;
    }
    
    private static Func<T, T[], T> CreateInvokerForStaticCompiler(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
      case 0:
        var d0 = DelegateHelper.CreateDelegate<Func<T>>(null, t, s);
        return (_, args) => d0();
      case 1:
        var d1 = DelegateHelper.CreateDelegate<Func<T, T>>(null, t, s);
        return (_, args) => d1(args[0]);
      case 2:
        var d2 = DelegateHelper.CreateDelegate<Func<T, T, T>>(null, t, s);
        return (_, args) => d2(args[0], args[1]);
      case 3:
        var d3 = DelegateHelper.CreateDelegate<Func<T, T, T, T>>(null, t, s);
        return (_, args) => d3(args[0], args[1], args[2]);
      case 4:
        var d4 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T>>(null, t, s);
        return (_, args) => d4(args[0], args[1], args[2], args[3]);
      case 5:
        var d5 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d5(args[0], args[1], args[2], args[3], args[4]);
      case 6:
        var d6 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d6(args[0], args[1], args[2], args[3], args[4], args[5]);
      case 7:
        var d7 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d7(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
      case 8:
        var d8 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d8(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
      case 9:
        var d9 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d9(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
      case 10:
        var d10 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d10(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
      case 11:
        var d11 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d11(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
      case 12:
        var d12 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d12(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
      case 13:
        var d13 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d13(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
      case 14:
        var d14 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d14(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
      case 15:
        var d15 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d15(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);
      case 16:
        var d16 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (_, args) => d16(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]);
      }    
      return null;
    }
    
    private static Func<MemberInfo, T, T[], T> CreateInvokerForStaticGenericCompiler(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
      case 1:
        var d1 = DelegateHelper.CreateDelegate<Func<MemberInfo, T>>(null, t, s);
        return (member, _, args) => d1(member);
      case 2:
        var d2 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T>>(null, t, s);
        return (member, _, args) => d2(member, args[0]);
      case 3:
        var d3 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T>>(null, t, s);
        return (member, _, args) => d3(member, args[0], args[1]);
      case 4:
        var d4 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T>>(null, t, s);
        return (member, _, args) => d4(member, args[0], args[1], args[2]);
      case 5:
        var d5 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d5(member, args[0], args[1], args[2], args[3]);
      case 6:
        var d6 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d6(member, args[0], args[1], args[2], args[3], args[4]);
      case 7:
        var d7 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d7(member, args[0], args[1], args[2], args[3], args[4], args[5]);
      case 8:
        var d8 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d8(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
      case 9:
        var d9 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d9(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
      case 10:
        var d10 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d10(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
      case 11:
        var d11 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d11(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
      case 12:
        var d12 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d12(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
      case 13:
        var d13 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d13(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
      case 14:
        var d14 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d14(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
      case 15:
        var d15 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d15(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
      case 16:
        var d16 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (member, _, args) => d16(member, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);
      }
      return null;
    }
  }
}