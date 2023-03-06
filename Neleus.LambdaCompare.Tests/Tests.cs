using System;
using System.Globalization;
using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Reflection;

namespace Neleus.LambdaCompare.Tests
{
    public class Tests
    {
        [Fact]
        public void BasicConst()
        {
            var const1 = 25;
            var f1 = (Expression<Func<int, string, string>>)((first, second) =>
                $"{(first + const1).ToString(CultureInfo.InvariantCulture)}{first + second}{"some const value".ToUpper()}{const1}");

            var const2 = "some const value";
            var const3 = "{0}{1}{2}{3}";
            var f2 = (Expression<Func<int, string, string>>)((i, s) =>
                string.Format(const3, (i + 25).ToString(CultureInfo.InvariantCulture), i + s, const2.ToUpper(), 25));

            Lambda.Eq(f1, f2).Should().BeTrue();
        }

        [Fact]
        public void PropAndMethodCall()
        {
            var f1 = (Expression<Func<Uri, bool>>)(arg1 => Uri.IsWellFormedUriString(arg1.ToString(), UriKind.Absolute));
            var f2 = (Expression<Func<Uri, bool>>)(u => Uri.IsWellFormedUriString(u.ToString(), UriKind.Absolute));

            Lambda.Eq(f1, f2).Should().BeTrue();
        }

        [Fact]
        public void MemberInitWithConditional()
        {
            var port = 443;
            var f1 = (Expression<Func<Uri, UriBuilder>>)(x => new UriBuilder(x)
            {
                Port = port,
                Host = string.IsNullOrEmpty(x.Host) ? "abc" : "def"
            });

            var isSecure = true;
            var f2 = (Expression<Func<Uri, UriBuilder>>)(u => new UriBuilder(u)
            {
                Host = string.IsNullOrEmpty(u.Host) ? "abc" : "def",
                Port = isSecure ? 443 : 80
            });

            Lambda.Eq(f1, f2).Should().BeTrue();
        }

        [Fact]
        public void AnonymousType_Not_Supported()
        {
            var f1 = (Expression<Func<Uri, object>>)(x => new { Port = 443, x.Host, Addr = x.AbsolutePath });
            var f2 = (Expression<Func<Uri, object>>)(u => new { u.Host, Port = 443, Addr = u.AbsolutePath });

            var sut = (Func<bool>)(() => Lambda.Eq(f1, f2));

            sut.Should()
                .Throw<NotImplementedException>()
                .WithMessage("Comparison of Anonymous Types is not supported");
        }

        [Fact]
        public void Nulls_are_compared_correctly()
        {
            var f1 = (Expression<Func<Uri, string>>)(_ => "");
            var f2 = (Expression<Func<Uri, string>>)(null);
            var f3 = (Expression<Func<Uri, string>>)(null);

            Lambda.Eq(f1, f2).Should().BeFalse();
            Lambda.Eq(f2, f3).Should().BeTrue();
        }

        [Fact]
        public void Calculations_are_compared_correctly()
        {
            var f1 = (Expression<Func<Uri, int>>)(_ => 1 + 2);
            var f2 = (Expression<Func<Uri, int>>)(_ => 3);

            Lambda.Eq(f1, f2).Should().BeTrue();
        }

        [Fact]
        public void Member_init_expressions_are_compared_correctly()
        {
            var addMethod = typeof(List<string>).GetTypeInfo().GetDeclaredMethod("Add");

            var bindingMessages = Expression.ListBind(
                typeof(Node).GetProperty("Messages"),
                Expression.ElementInit(addMethod, Expression.Constant("Constant1"))
            );

            var bindingDescriptions = Expression.ListBind(
                typeof(Node).GetProperty("Descriptions"),
                Expression.ElementInit(addMethod, Expression.Constant("Constant2"))
            );

            Expression e1 = Expression.MemberInit(
                Expression.New(typeof(Node)),
                new List<MemberBinding> { bindingMessages }
            );

            Expression e2 = Expression.MemberInit(
                Expression.New(typeof(Node)),
                new List<MemberBinding> { bindingMessages, bindingDescriptions }
            );

            Lambda.ExpressionsEqual(e1, e2).Should().BeFalse();
            Lambda.ExpressionsEqual(e1, e1).Should().BeTrue();
        }

        [Fact]
        public void Default_expressions_are_compared_correctly()
        {
            Expression e1 = Expression.Default(typeof(int));
            Expression e2 = Expression.Default(typeof(int));
            Expression e3 = Expression.Default(typeof(string));

            Lambda.ExpressionsEqual(e1, e2).Should().BeTrue();
            Lambda.ExpressionsEqual(e1, e3).Should().BeFalse();
            Lambda.ExpressionsEqual(e1, e1).Should().BeTrue();
        }

        [Fact]
        public void Array_constant_expressions_are_compared_correctly()
        {
            var e1 = Expression.Constant(new[] { 1, 2, 3 });
            var e2 = Expression.Constant(new[] { 1, 2, 3 });
            var e3 = Expression.Constant(new[] { 1, 2, 4 });

            Lambda.ExpressionsEqual(e1, e2).Should().BeTrue();
            Lambda.ExpressionsEqual(e1, e3).Should().BeFalse();
        }

        private class Node
        {
            public List<string> Messages { set; get; }

            public List<string> Descriptions { set; get; }
        }
    }
}