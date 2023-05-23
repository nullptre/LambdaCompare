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

        private class ClassForTesting
        {
            public object Property1 { get; set; }
            public object Property2 { get; set; }
            public Dictionary<int, int> DictProperty { get; set; }
        }

        private class AdditionalClassForTesting
        {
            public DateTime? Date { get; set; }
        }

        [TestMethod]
        public void X()
        {
            // Object initialization
            Expression<Func<IGrouping<int, object>, ClassForTesting>> ox = x => new ClassForTesting
            {
                Property1 = x.Sum(d => 1),
                Property2 = x.Sum(d => 0)
            };
            Expression<Func<IGrouping<int, object>, ClassForTesting>> oy = x => new ClassForTesting
            {
                Property1 = x.Sum(d => 1),
                Property2 = x.Sum(d => 0)
            };
            Assert.IsTrue(Lambda.Eq(ox, oy));

            oy = x => new ClassForTesting
            {
                Property2 = x.Sum(d => 0),
                Property1 = x.Sum(d => 1)
            };
            Assert.IsTrue(Lambda.Eq(ox, oy));

            ox = x => new ClassForTesting();
            oy = x => new ClassForTesting();
            Assert.IsTrue(Lambda.Eq(ox, oy));


            // List initialization
            Expression<Func<object, List<int>>> lx = g => new List<int> { 3, 7, 30 };
            Expression<Func<object, List<int>>> ly = g => new List<int> { 3, 7, 30 };
            Assert.IsTrue(Lambda.Eq(lx, ly));

            ly = g => new List<int> { 7, 3, 30 };
            Assert.IsTrue(Lambda.Eq(lx, ly));

            lx = g => new List<int>();
            ly = g => new List<int>();
            Assert.IsTrue(Lambda.Eq(lx, ly));


            // Dictionary initialization
            Expression<Func<object, Dictionary<int, int>>> dx = x => new Dictionary<int, int> { { 3, 33 }, { 7, 77 }, { 30, 333 } };
            Expression<Func<object, Dictionary<int, int>>> dy = x => new Dictionary<int, int> { { 3, 33 }, { 7, 77 }, { 30, 333 } };
            Assert.IsTrue(Lambda.Eq(dx, dy));

            dy = g => new Dictionary<int, int> { { 7, 77 }, { 3, 33 }, { 30, 333 } };
            Assert.IsTrue(Lambda.Eq(dx, dy));

            dx = g => new Dictionary<int, int>();
            dy = g => new Dictionary<int, int>();
            Assert.IsTrue(Lambda.Eq(dx, dy));


            // Dictionary initialization inside grouping
            var utcNow = DateTime.UtcNow;
            var limitDate3 = utcNow.AddDays(-3);
            var limitDate7 = utcNow.AddDays(-7);
            var limitDate30 = utcNow.AddDays(-30);
            Expression<Func<IGrouping<int, AdditionalClassForTesting>, ClassForTesting>> gx = g => new ClassForTesting
            {
                DictProperty = new Dictionary<int, int>
                {
                    { 3, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate3 ? 1 : 0) },
                    { 7, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate7 ? 1 : 0) },
                    { 30, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate30 ? 1 : 0) }
                }
            };
            Expression<Func<IGrouping<int, AdditionalClassForTesting>, ClassForTesting>> gy = g => new ClassForTesting
            {
                DictProperty = new Dictionary<int, int>
                {
                    { 3, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate3 ? 1 : 0) },
                    { 7, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate7 ? 1 : 0) },
                    { 30, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate30 ? 1 : 0) }
                }
            };
            Assert.IsTrue(Lambda.Eq(gx, gy));

            gy = g => new ClassForTesting
            {
                DictProperty = new Dictionary<int, int>
                {
                    { 7, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate7 ? 1 : 0) },
                    { 3, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate3 ? 1 : 0) },
                    { 30, g.Sum(d => d.Date.HasValue && d.Date.Value < limitDate30 ? 1 : 0) }
                }
            };
            Assert.IsTrue(Lambda.Eq(gx, gy));

            gx = g => new ClassForTesting { DictProperty = new Dictionary<int, int>() };
            gy = g => new ClassForTesting { DictProperty = new Dictionary<int, int>() };
            Assert.IsTrue(Lambda.Eq(gx, gy));
        }
    }
}
