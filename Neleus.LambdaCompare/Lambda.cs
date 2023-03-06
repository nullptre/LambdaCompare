using System;
using System.Linq.Expressions;

namespace Neleus.LambdaCompare
{
    /// <summary>
    /// Extensions for comparing two expressions
    /// </summary>
    public static class Lambda
    {
        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource, TValue>(
            Expression<Func<TSource, TValue>> x,
            Expression<Func<TSource, TValue>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource1, TSource2, TValue>(
            Expression<Func<TSource1, TSource2, TValue>> x,
            Expression<Func<TSource1, TSource2, TValue>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource1, TSource2, TSource3, TValue>(
            Expression<Func<TSource1, TSource2, TSource3, TValue>> x,
            Expression<Func<TSource1, TSource2, TSource3, TValue>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource1, TSource2, TSource3, TSource4, TValue>(
            Expression<Func<TSource1, TSource2, TSource3, TSource4, TValue>> x,
            Expression<Func<TSource1, TSource2, TSource3, TSource4, TValue>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource1, TSource2, TSource3, TSource4, TSource5, TValue>(
            Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TValue>> x,
            Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TValue>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Gets predicate which compares any expression to <paramref name="y"/> expression.
        /// This can be useful for mock verifications, like Moq.It.Is() method which accepts predicate expressions for agrument constraints.
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static Expression<Func<Expression<Func<TSource, TValue>>, bool>> Eq<TSource, TValue>(Expression<Func<TSource, TValue>> y)
        {
            return x => ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Gets predicate which compares any expression to <paramref name="y"/> expression.
        /// This can be useful for mock verifications, like Moq.It.Is() method which accepts predicate expressions for agrument constraints.
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static Expression<Func<Expression<Func<TSource1, TSource2, TValue>>, bool>> Eq<TSource1, TSource2, TValue>(Expression<Func<TSource1, TSource2, TValue>> y)
        {
            return x => ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Gets predicate which compares any expression to <paramref name="y"/> expression.
        /// This can be useful for mock verifications, like Moq.It.Is() method which accepts predicate expressions for agrument constraints.
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static Expression<Func<Expression<Func<TSource1, TSource2, TSource3, TValue>>, bool>> Eq<TSource1, TSource2, TSource3, TValue>(Expression<Func<TSource1, TSource2, TSource3, TValue>> y)
        {
            return x => ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Gets predicate which compares any expression to <paramref name="y"/> expression.
        /// This can be useful for mock verifications, like Moq.It.Is() method which accepts predicate expressions for agrument constraints.
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static Expression<Func<Expression<Func<TSource1, TSource2, TSource3, TSource4, TValue>>, bool>> Eq<TSource1, TSource2, TSource3, TSource4, TValue>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TValue>> y)
        {
            return x => ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource>(
            Expression<Action<TSource>> x,
            Expression<Action<TSource>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource1, TSource2>(
            Expression<Action<TSource1, TSource2>> x,
            Expression<Action<TSource1, TSource2>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource1, TSource2, TSource3>(
            Expression<Action<TSource1, TSource2, TSource3>> x,
            Expression<Action<TSource1, TSource2, TSource3>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource1, TSource2, TSource3, TSource4>(
            Expression<Action<TSource1, TSource2, TSource3, TSource4>> x,
            Expression<Action<TSource1, TSource2, TSource3, TSource4>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Compares two expressions
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static bool Eq<TSource1, TSource2, TSource3, TSource4, TSource5>(
            Expression<Action<TSource1, TSource2, TSource3, TSource4, TSource5>> x,
            Expression<Action<TSource1, TSource2, TSource3, TSource4, TSource5>> y)
        {
            return ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Gets predicate which compares any expression to <paramref name="y"/> expression.
        /// This can be useful for mock verifications, like Moq.It.Is() method which accepts predicate expressions for agrument constraints.
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static Expression<Func<Expression<Action<TSource>>, bool>> Eq<TSource>(Expression<Action<TSource>> y)
        {
            return x => ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Gets predicate which compares any expression to <paramref name="y"/> expression.
        /// This can be useful for mock verifications, like Moq.It.Is() method which accepts predicate expressions for agrument constraints.
        /// </summary>
        public static Expression<Func<Expression<Action<TSource1, TSource2>>, bool>> Eq<TSource1, TSource2>(Expression<Action<TSource1, TSource2>> y)
        {
            return x => ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Gets predicate which compares any expression to <paramref name="y"/> expression.
        /// This can be useful for mock verifications, like Moq.It.Is() method which accepts predicate expressions for agrument constraints.
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static Expression<Func<Expression<Action<TSource1, TSource2, TSource3>>, bool>> Eq<TSource1, TSource2, TSource3>(Expression<Action<TSource1, TSource2, TSource3>> y)
        {
            return x => ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Gets predicate which compares any expression to <paramref name="y"/> expression.
        /// This can be useful for mock verifications, like Moq.It.Is() method which accepts predicate expressions for agrument constraints.
        /// It's a strongly typed version of <see cref="ExpressionsEqual"/>
        /// </summary>
        public static Expression<Func<Expression<Action<TSource1, TSource2, TSource3, TSource4>>, bool>> Eq<TSource1, TSource2, TSource3, TSource4>(Expression<Action<TSource1, TSource2, TSource3, TSource4>> y)
        {
            return x => ExpressionsEqual(x, y);
        }

        /// <summary>
        /// Generic version that compares any expressions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool ExpressionsEqual(Expression x, Expression y)
        {
            return Comparer.ExpressionsEqual(x, y, null, null);
        }
    }
}