using System;
using System.Globalization;
using System.Linq.Expressions;
using Xunit;
using FluentAssertions;

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

            Assert.True(Lambda.Eq(f1, f2));
        }

        [Fact]
        public void PropAndMethodCall()
        {
            var f1 = (Expression<Func<Uri, bool>>)(arg1 => Uri.IsWellFormedUriString(arg1.ToString(), UriKind.Absolute));
            var f2 = (Expression<Func<Uri, bool>>)(u => Uri.IsWellFormedUriString(u.ToString(), UriKind.Absolute));

            Assert.True(Lambda.Eq(f1, f2));
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

            Assert.True(Lambda.Eq(f1, f2));
        }

        [Fact]
        public void AnonymousType()
        {
            // Arrange
            var f1 = (Expression<Func<Uri, object>>)(x => new { Port = 443, x.Host, Addr = x.AbsolutePath });
            var f2 = (Expression<Func<Uri, object>>)(u => new { u.Host, Port = 443, Addr = u.AbsolutePath });

            // Act
            var sut = (Func<bool>)(() => Lambda.Eq(f1, f2));

            // Assert
            sut.Should().Throw<NotImplementedException>();
        }
    }
}