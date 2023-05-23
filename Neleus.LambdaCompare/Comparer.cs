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
            var valueY = TryCalculateConstant(y);

            if (valueX.IsDefined && valueY.IsDefined)
                return ValuesEqual(valueX.Value, valueY.Value);

            if (x.NodeType != y.NodeType
                || x.Type != y.Type)
            {
                if (IsAnonymousType(x.Type) && IsAnonymousType(y.Type))
                    throw new NotImplementedException("Comparison of Anonymous Types is not supported");
                return false;
            }

            if (x is LambdaExpression)
            {
                var lx = (LambdaExpression)x;
                var ly = (LambdaExpression)y;
                var paramsX = lx.Parameters;
                var paramsY = ly.Parameters;
                return CollectionsEqual(paramsX, paramsY, lx, ly) && ExpressionsEqual(lx.Body, ly.Body, lx, ly);
            }
            if (x is MemberExpression)
            {
                var mex = (MemberExpression)x;
                var mey = (MemberExpression)y;
                return Equals(mex.Member, mey.Member) && ExpressionsEqual(mex.Expression, mey.Expression, rootX, rootY);
            }
            if (x is BinaryExpression)
            {
                var bx = (BinaryExpression)x;
                var by = (BinaryExpression)y;
                return bx.Method == by.Method && ExpressionsEqual(bx.Left, by.Left, rootX, rootY) && ExpressionsEqual(bx.Right, by.Right, rootX, rootY);
            }
            if (x is UnaryExpression)
            {
                var ux = (UnaryExpression)x;
                var uy = (UnaryExpression)y;
                return ux.Method == uy.Method && ExpressionsEqual(ux.Operand, uy.Operand, rootX, rootY);
            }
            if (x is ParameterExpression)
            {
                var px = (ParameterExpression)x;
                var py = (ParameterExpression)y;
                return rootX.Parameters.IndexOf(px) == rootY.Parameters.IndexOf(py);
            }
            if (x is MethodCallExpression)
            {
                var cx = (MethodCallExpression)x;
                var cy = (MethodCallExpression)y;
                return cx.Method == cy.Method
                       && ExpressionsEqual(cx.Object, cy.Object, rootX, rootY)
                       && CollectionsEqual(cx.Arguments, cy.Arguments, rootX, rootY);
            }
            if (x is MemberInitExpression)
            {
                var mix = (MemberInitExpression)x;
                var miy = (MemberInitExpression)y;
                return ExpressionsEqual(mix.NewExpression, miy.NewExpression, rootX, rootY)
                       && MemberInitsEqual(mix.Bindings, miy.Bindings, rootX, rootY);
            }
            if (x is NewArrayExpression)
            {
                var nx = (NewArrayExpression)x;
                var ny = (NewArrayExpression)y;
                return CollectionsEqual(nx.Expressions, ny.Expressions, rootX, rootY);
            }
            if (x is NewExpression)
            {
                var nx = (NewExpression)x;
                var ny = (NewExpression)y;
                return Equals(nx.Constructor, ny.Constructor)
                       && CollectionsEqual(nx.Arguments, ny.Arguments, rootX, rootY)
                       && (nx.Members == null && ny.Members == null
                           || nx.Members != null && ny.Members != null && CollectionsEqual(nx.Members, ny.Members));
            }
            if (x is ConditionalExpression)
            {
                var cx = (ConditionalExpression)x;
                var cy = (ConditionalExpression)y;
                return ExpressionsEqual(cx.Test, cy.Test, rootX, rootY)
                       && ExpressionsEqual(cx.IfFalse, cy.IfFalse, rootX, rootY)
                       && ExpressionsEqual(cx.IfTrue, cy.IfTrue, rootX, rootY);
            }
            if (x is ListInitExpression)
            {
                var lix = (ListInitExpression)x;
                var liy = (ListInitExpression)y;
                return ExpressionsEqual(lix.NewExpression, liy.NewExpression, rootX, rootY)
                       && ListInitsEqual(lix.Initializers, liy.Initializers, rootX, rootY);
            }

            throw new NotImplementedException(x.ToString());
        }

        private static bool IsAnonymousType(Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
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

        /// <summary>
        /// Based on https://stackoverflow.com/questions/26680849/comparing-multivariable-boolean-functions
        /// </summary>
        private static bool ListInitsEqual(ICollection<ElementInit> bx, ICollection<ElementInit> by, LambdaExpression rootX, LambdaExpression rootY)
        {
            if (bx.Count != by.Count)
            {
                return false;
            }

            return
                bx.OrderBy(b => b.Arguments.FirstOrDefault()?.ToString()).Select((b, i) => new { b.AddMethod, b.Arguments, Index = i })
                    .Join(
                        by.OrderBy(b => b.Arguments.FirstOrDefault()?.ToString()).Select((b, i) => new { b.AddMethod, b.Arguments, Index = i }),
                        o => o.Index, o => o.Index, (xe, ye) => new { X = xe, XAddMethod = xe.AddMethod, XArgs = xe.Arguments, Y = ye, YAddMethod = ye.AddMethod, YArgs = ye.Arguments })
                    .All(o =>
                        Equals(o.X, o.Y) ||
                        Equals(o.XAddMethod, o.YAddMethod) && o.XArgs.Count == o.YArgs.Count && o.XArgs.Select((_, i) => ExpressionsEqual(o.XArgs[i], o.YArgs[i], rootX, rootY)).All(r => r));
        }

        private static bool ValuesEqual(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is ICollection && y is ICollection)
                return CollectionsEqual((ICollection)x, (ICollection)y);

            return Equals(x, y);
        }

        private static ConstantValue TryCalculateConstant(Expression e)
        {
            if (e is ConstantExpression)
                return new ConstantValue(true, ((ConstantExpression)e).Value);
            if (e is MemberExpression)
            {
                var me = (MemberExpression)e;
                var parentValue = TryCalculateConstant(me.Expression);
                if (parentValue.IsDefined)
                {
                    var result =
                        me.Member is FieldInfo
                            ? ((FieldInfo)me.Member).GetValue(parentValue.Value)
                            : ((PropertyInfo)me.Member).GetValue(parentValue.Value);
                    return new ConstantValue(true, result);
                }
            }
            if (e is NewArrayExpression)
            {
                var ae = ((NewArrayExpression)e);
                var result = ae.Expressions.Select(TryCalculateConstant);
                if (result.All(i => i.IsDefined))
                    return new ConstantValue(true, result.Select(i => i.Value).ToArray<object>());
            }
            if (e is ConditionalExpression)
            {
                var ce = (ConditionalExpression)e;
                var evaluatedTest = TryCalculateConstant(ce.Test);
                if (evaluatedTest.IsDefined)
                {
                    return TryCalculateConstant(Equals(evaluatedTest.Value, true) ? ce.IfTrue : ce.IfFalse);
                }
            }

            return default(ConstantValue);
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

        private struct ConstantValue
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
