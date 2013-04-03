using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace RethinkDb.Test
{
    [TestFixture]
    public class BlankTests : TestBase
    {
        [Test]
        public void DbCreateTest()
        {
            DoDbCreateTest().Wait();
        }

        private async Task DoDbCreateTest()
        {
            var resp = await connection.RunAsync(Query.DbCreate("test"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Created, Is.EqualTo(1));

            var dbList = await connection.RunAsync(Query.DbList());
            Assert.That(dbList, Is.Not.Null);
            Assert.That(dbList, Contains.Item("test"));

            resp = await connection.RunAsync(Query.DbDrop("test"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Dropped, Is.EqualTo(1));
        }

        [Test]
        public void ExprPassthrough()
        {
            DoExprPassthrough().Wait();
        }

        private async Task DoExprPassthrough()
        {
            var obj = new TestObject() {
                Id = "123",
                Name = "456",
                SomeNumber = 789,
                Children = new TestObject[] {
                    new TestObject() { Id = "987", Name = "654", SomeNumber = 321 },
                },
            };
            var resp = await connection.RunAsync(Query.ExprObj(obj));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp, Is.EqualTo(obj));
        }

        [Test]
        public void ExprExpression()
        {
            DoExprExpression(() => 1.0 + 2.0, 3.0).Wait();
        }

        private async Task DoExprExpression<T>(Expression<Func<T>> objectExpr, T value)
        {
            var resp = await connection.RunAsync(Query.ExprFunc(objectExpr));
            Assert.That(resp, Is.EqualTo(value));
        }

        [Test]
        public void ExprSequence()
        {
            DoExprSequence(new double[] { 1, 2, 3 }).Wait();
        }

        private async Task DoExprSequence<T>(IEnumerable<T> enumerable)
        {
            var asyncEnumerable = connection.RunAsync(Query.ExprSeq(enumerable));
            var count = 0;
            while (true)
            {
                if (!await asyncEnumerable.MoveNext())
                    break;
                ++count;
                Assert.That(asyncEnumerable.Current, Is.EqualTo(count));
            }
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public void ExprNth()
        {
            DoExprNth(new double[] { 1, 2, 3 }).Wait();
        }

        private async Task DoExprNth<T>(IEnumerable<T> enumerable)
        {
            var resp = await connection.RunAsync(Query.ExprSeq(enumerable).Nth(1));
            Assert.That(resp, Is.EqualTo(2));
        }

        [Test]
        public void ExprDistinct()
        {
            DoExprDistinct().Wait();
        }

        private async Task DoExprDistinct()
        {
            var asyncEnumerable = connection.RunAsync(Query.ExprSeq(new double[] { 1, 2, 3, 2, 1 }).Distinct());
            var count = 0;
            while (true)
            {
                if (!await asyncEnumerable.MoveNext())
                    break;
                ++count;
                Assert.That(asyncEnumerable.Current, Is.EqualTo(count));
            }
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public void AnonymousTypeExpression()
        {
            var anonValue = connection.Run(Query.ExprFunc(() => new { Prop1 = 1.0, Prop2 = 2.0, Prop3 = "3" }));
            Assert.That(anonValue.Prop1, Is.EqualTo(1.0));
            Assert.That(anonValue.Prop2, Is.EqualTo(2.0));
            Assert.That(anonValue.Prop3, Is.EqualTo("3"));
        }

        [Test]
        public void AnonymousTypeValue()
        {
            var anonValue = connection.Run(Query.ExprObj(new { Prop1 = 1.0, Prop2 = 2.0, Prop3 = "3" }));
            Assert.That(anonValue.Prop1, Is.EqualTo(1.0));
            Assert.That(anonValue.Prop2, Is.EqualTo(2.0));
            Assert.That(anonValue.Prop3, Is.EqualTo("3"));
        }

        [Test]
        public void ExprJs()
        {
            var num = connection.Run(Query.ExprFunc(Query.Js<double>("178")));
            Assert.That(num, Is.EqualTo(178.0));
        }
    }
}

