using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Neleus.LambdaCompare
{
    internal static class Comparer
    {
        public static bool ExpressionsEqual(Expression x, Expression y, LambdaExpression rootX, LambdaExpression rootY)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;

            var valueX = TryCalculateConstant(x);
            if (valueX.IsDefined)
            {
                var valueY = TryCalculateConstant(y);
                if (valueY.IsDefined)
                {
                    return ValuesEqual(valueX.Value, valueY.Value);
                }
            }

            if (x.NodeType != y.NodeType
                || x.Type != y.Type)
            {
                if (IsAnonymousType(x.Type) && IsAnonymousType(y.Type))
                    throw new NotImplementedException("Comparison of Anonymous Types is not supported");
                return false;
            }

            if (x is LambdaExpression lx)
            {
                var ly = (LambdaExpression)y;
                var paramsX = lx.Parameters;
                var paramsY = ly.Parameters;
                return CollectionsEqual(paramsX, paramsY, lx, ly) && ExpressionsEqual(lx.Body, ly.Body, lx, ly);
            }
            else if (x is MemberExpression mex)
            {
                var mey = (MemberExpression)y;
                return Equals(mex.Member, mey.Member) && ExpressionsEqual(mex.Expression, mey.Expression, rootX, rootY);
            }
            else if (x is BinaryExpression bx)
            {
                var by = (BinaryExpression)y;
                return bx.Method == by.Method && ExpressionsEqual(bx.Left, by.Left, rootX, rootY) && ExpressionsEqual(bx.Right, by.Right, rootX, rootY);
            }
            else if (x is UnaryExpression ux)
            {
                var uy = (UnaryExpression)y;
                return ux.Method == uy.Method && ExpressionsEqual(ux.Operand, uy.Operand, rootX, rootY);
            }
            else if (x is ParameterExpression px)
            {
                var py = (ParameterExpression)y;
                return rootX.Parameters.IndexOf(px) == rootY.Parameters.IndexOf(py);
            }
            else if (x is MethodCallExpression mcx)
            {
                var mcy = (MethodCallExpression)y;
                return mcx.Method == mcy.Method
                       && ExpressionsEqual(mcx.Object, mcy.Object, rootX, rootY)
                       && CollectionsEqual(mcx.Arguments, mcy.Arguments, rootX, rootY);
            }
            else if (x is MemberInitExpression mix)
            {
                var miy = (MemberInitExpression)y;
                return ExpressionsEqual(mix.NewExpression, miy.NewExpression, rootX, rootY)
                       && MemberInitsEqual(mix.Bindings, miy.Bindings, rootX, rootY);
            }
            else if (x is NewArrayExpression nax)
            {
                var nay = (NewArrayExpression)y;
                return CollectionsEqual(nax.Expressions, nay.Expressions, rootX, rootY);
            }
            else if (x is NewExpression nx)
            {
                var ny = (NewExpression)y;
                return Equals(nx.Constructor, ny.Constructor)
                       && CollectionsEqual(nx.Arguments, ny.Arguments, rootX, rootY)
                       && ((nx.Members == null && ny.Members == null)
                           || (nx.Members != null && ny.Members != null && CollectionsEqual(nx.Members, ny.Members)));
            }
            else if (x is ConditionalExpression cx)
            {
                var cy = (ConditionalExpression)y;
                return ExpressionsEqual(cx.Test, cy.Test, rootX, rootY)
                       && ExpressionsEqual(cx.IfFalse, cy.IfFalse, rootX, rootY)
                       && ExpressionsEqual(cx.IfTrue, cy.IfTrue, rootX, rootY);
            }
            else if (x is DefaultExpression)
            {
                return true;
            }

            throw new NotImplementedException(x.ToString());
        }

        private static bool IsAnonymousType(Type type)
        {
            return type.FullName.Contains("AnonymousType") &&
                type.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
        }

        private static bool MemberInitsEqual(ICollection<MemberBinding> bx, ICollection<MemberBinding> by, LambdaExpression rootX, LambdaExpression rootY)
        {
            if (bx.Count != by.Count)
            {
                return false;
            }

            if (bx.Concat(by).Any(b => b.BindingType != MemberBindingType.Assignment))
                throw new NotImplementedException("Only MemberBindingType.Assignment is supported");

            return
                bx.Cast<MemberAssignment>().OrderBy(b => b.Member.Name).Select((b, i) => new { Expr = b.Expression, b.Member, Index = i })
                    .Join(
                        by.Cast<MemberAssignment>().OrderBy(b => b.Member.Name).Select((b, i) => new { Expr = b.Expression, b.Member, Index = i }),
                        o => o.Index, o => o.Index, (xe, ye) => new { XExpr = xe.Expr, XMember = xe.Member, YExpr = ye.Expr, YMember = ye.Member })
                    .All(o => Equals(o.XMember, o.YMember) && ExpressionsEqual(o.XExpr, o.YExpr, rootX, rootY));
        }

        private static bool ValuesEqual(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is ICollection collectionX && y is ICollection collectionY)
                return CollectionsEqual(collectionX, collectionY);

            return Equals(x, y);
        }

        private static ConstantValue TryCalculateConstant(Expression e)
        {
            if (e is ConstantExpression constantExpression)
            {
                return new ConstantValue(true, constantExpression.Value);
            }
            else if (e is MemberExpression memberExpression)
            {
                var parentValue = TryCalculateConstant(memberExpression.Expression);
                if (parentValue.IsDefined)
                {
                    var result =
                        memberExpression.Member is FieldInfo info
                            ? info.GetValue(parentValue.Value)
                            : ((PropertyInfo)memberExpression.Member).GetValue(parentValue.Value);
                    return new ConstantValue(true, result);
                }
            }
            else if (e is NewArrayExpression newArrayExpression)
            {
                var result = newArrayExpression.Expressions.Select(TryCalculateConstant);
                if (result.All(i => i.IsDefined))
                    return new ConstantValue(true, result.Select(i => i.Value).ToArray());
            }
            else if (e is ConditionalExpression conditionalExpression)
            {
                var evaluatedTest = TryCalculateConstant(conditionalExpression.Test);
                if (evaluatedTest.IsDefined)
                {
                    return TryCalculateConstant(Equals(evaluatedTest.Value, true)
                        ? conditionalExpression.IfTrue
                        : conditionalExpression.IfFalse);
                }
            }

            return default;
        }

        private static bool CollectionsEqual(IEnumerable<Expression> x, IEnumerable<Expression> y, LambdaExpression rootX, LambdaExpression rootY)
        {
            return x.Count() == y.Count()
                   && x.Select((e, i) => new { Expr = e, Index = i })
                       .Join(y.Select((e, i) => new { Expr = e, Index = i }),
                           o => o.Index, o => o.Index, (xe, ye) => new { X = xe.Expr, Y = ye.Expr })
                       .All(o => ExpressionsEqual(o.X, o.Y, rootX, rootY));
        }

        private static bool CollectionsEqual(ICollection x, ICollection y)
        {
            return x.Count == y.Count
                   && x.Cast<object>().Select((e, i) => new { Expr = e, Index = i })
                       .Join(y.Cast<object>().Select((e, i) => new { Expr = e, Index = i }),
                           o => o.Index, o => o.Index, (xe, ye) => new { X = xe.Expr, Y = ye.Expr })
                       .All(o => Equals(o.X, o.Y));
        }

        private readonly struct ConstantValue
        {
            public ConstantValue(bool isDefined, object value)
                : this()
            {
                IsDefined = isDefined;
                Value = value;
            }

            public bool IsDefined { get; }

            public object Value { get; }
        }
    }
}