using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroIoc.Tests
{
    [TestClass]
    public class TupleFixture
    {
        [TestMethod]
        public void TupleEqualsNullReturnsFalse()
        {
            var testTuple = new Tuple<string, int>("this is a test", 42);

            Assert.IsFalse(testTuple.Equals(null));
        }

        [TestMethod]
        public void TupleEqualsItselfReturnsTrue()
        {
            var testTuple = new Tuple<string, int>("this is a test", 42);

            Assert.IsTrue(testTuple.Equals(testTuple));
        }

        [TestMethod]
        public void EqualsOperatorReturnsTrueIfTuplesComposedOfSamePieces()
        {
            var tuple1 = new Tuple<string, int>("this is a test", 42);
            var tuple2 = new Tuple<string, int>("this is a test", 42);

            Assert.IsTrue(tuple1.Equals(tuple2));
        }

        [TestMethod]
        public void EqualsOperatorReturnsFalseIfTuplesContainDifferentItem1()
        {
            var tuple1 = new Tuple<string, int>("this is a test", 42);
            var tuple2 = new Tuple<string, int>("this is another test", 42);

            Assert.IsFalse(tuple1.Equals(tuple2));
        }

        [TestMethod]
        public void EqualsOperatorReturnsFalseIfTuplesContainDifferentItem2()
        {
            var tuple1 = new Tuple<string, int>("this is a test", 42);
            var tuple2 = new Tuple<string, int>("this is a test", 9);

            Assert.IsFalse(tuple1.Equals(tuple2));
        }
    }
}
