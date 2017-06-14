using System;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neleus.LambdaCompare.Test
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void BasicConst()
        {
            var f1 = GetBasicExpr1();
            var f2 = GetBasicExpr2();
            Assert.IsTrue(Lambda.Eq(f1, f2));
        }

        [TestMethod]
        public void PropAndMethodCall()
        {
            var f1 = GetPropAndMethodExpr1();
            var f2 = GetPropAndMethodExpr2();
            Assert.IsTrue(Lambda.Eq(f1, f2));
        }

        [TestMethod]
        public void MemberInitWithConditional()
        {
            var f1 = GetMemberInitExpr1();
            var f2 = GetMemberInitExpr2();
            Assert.IsTrue(Lambda.Eq(f1, f2));
        }

        [TestMethod]
        public void AnonymousType()
        {
            var f1 = GetAnonymousExpr1();
            var f2 = GetAnonymousExpr2();
            Assert.Inconclusive("Anonymous Types are not supported");
        }

        private static Expression<Func<int, string, string>> GetBasicExpr2()
        {
            var const2 = "some const value";
            var const3 = "{0}{1}{2}{3}";
            return (i, s) =>
                string.Format(const3, (i + 25).ToString(CultureInfo.InvariantCulture), i + s, const2.ToUpper(), 25);
        }

        private static Expression<Func<int, string, string>> GetBasicExpr1()
        {
            var const1 = 25;
            return (first, second) =>
                string.Format("{0}{1}{2}{3}", (first + const1).ToString(CultureInfo.InvariantCulture), first + second,
                    "some const value".ToUpper(), const1);
        }

        private static Expression<Func<Uri, bool>> GetPropAndMethodExpr2()
        {
            return u => Uri.IsWellFormedUriString(u.ToString(), UriKind.Absolute);
        }

        private static Expression<Func<Uri, bool>> GetPropAndMethodExpr1()
        {
            return arg1 => Uri.IsWellFormedUriString(arg1.ToString(), UriKind.Absolute);
        }

        private static Expression<Func<Uri, UriBuilder>> GetMemberInitExpr2()
        {
            var isSecure = true;
            return u => new UriBuilder(u) { Host = string.IsNullOrEmpty(u.Host) ? "abc" : "def", Port = isSecure ? 443 : 80 };
        }

        private static Expression<Func<Uri, UriBuilder>> GetMemberInitExpr1()
        {
            var port = 443;
            return x => new UriBuilder(x) { Port = port, Host = string.IsNullOrEmpty(x.Host) ? "abc" : "def" };
        }

        private static Expression<Func<Uri, object>> GetAnonymousExpr2()
        {
            return u => new { u.Host, Port = 443, Addr = u.AbsolutePath };
        }

        private static Expression<Func<Uri, object>> GetAnonymousExpr1()
        {
            return x => new { Port = 443, x.Host, Addr = x.AbsolutePath };
        }
    }
}
