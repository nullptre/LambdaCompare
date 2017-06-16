using System;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neleus.LambdaCompare.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void BasicConst()
        {
            var const1 = 25;
            var f1 = (Expression<Func<int, string, string>>) ((first, second) =>
                $"{(first + const1).ToString(CultureInfo.InvariantCulture)}{first + second}{"some const value".ToUpper()}{const1}");

            var const2 = "some const value";
            var const3 = "{0}{1}{2}{3}";
            var f2 = (Expression<Func<int, string, string>>) ((i, s) =>
                string.Format(const3, (i + 25).ToString(CultureInfo.InvariantCulture), i + s, const2.ToUpper(), 25));

            Assert.IsTrue(Lambda.Eq(f1, f2));
        }

        [TestMethod]
        public void PropAndMethodCall()
        {
            var f1 = (Expression<Func<Uri, bool>>) (arg1 => Uri.IsWellFormedUriString(arg1.ToString(), UriKind.Absolute));
            var f2 = (Expression<Func<Uri, bool>>) (u => Uri.IsWellFormedUriString(u.ToString(), UriKind.Absolute));

            Assert.IsTrue(Lambda.Eq(f1, f2));
        }

        [TestMethod]
        public void MemberInitWithConditional()
        {
            var port = 443;
            var f1 = (Expression<Func<Uri, UriBuilder>>) (x => new UriBuilder(x)
            {
                Port = port,
                Host = string.IsNullOrEmpty(x.Host) ? "abc" : "def"
            });

            var isSecure = true;
            var f2 = (Expression<Func<Uri, UriBuilder>>) (u => new UriBuilder(u)
            {
                Host = string.IsNullOrEmpty(u.Host) ? "abc" : "def",
                Port = isSecure ? 443 : 80
            });

            Assert.IsTrue(Lambda.Eq(f1, f2));
        }

        [TestMethod, Ignore]
        public void AnonymousType()
        {
            var f1 = (Expression<Func<Uri, object>>) (x => new { Port = 443, x.Host, Addr = x.AbsolutePath });
            var f2 = (Expression<Func<Uri, object>>) (u => new { u.Host, Port = 443, Addr = u.AbsolutePath });
            Assert.Inconclusive("Anonymous Types are not supported");
        }
    }
}
