using System;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Regex;

namespace RegexTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestOne()
        {
            var mapping = new
            {
                one = "Moses",
                two = "Bullrushes",
                three = "Snooty"
            };

            string result = RegexFormat.Rewrite("one two three", mapping);
            Assert.AreEqual("Moses Bullrushes Snooty",result);
            return;
        }

        [TestMethod]
        public void TestTwo()
        {
            var mapping = new
            {
                one = "Moses",
                two = "Bullrushes",
            };

            string result = RegexFormat.Rewrite("one two three", mapping);
            Assert.AreEqual("Moses Bullrushes three", result);
            return;
        }

        [TestMethod]
        public void TestThree()
        {
            var mapping = new
            {
                one = "Moses",
                two = "Bullrushes",
            };

            string result = RegexFormat.Rewrite("one twothree", mapping);
            Assert.AreEqual("Moses Bullrushesthree", result);
            return;
        }

        [TestMethod]
        public void TestFour()
        {
            var mapping = new
            {
                one = "Moses",
                two = "Bullrushes",
            };

            string result = RegexFormat.Rewrite("one twothree one", mapping);
            Assert.AreEqual("Moses Bullrushesthree Moses", result);
            return;
        }

        [TestMethod]
        public void TestRegex()
        {
            var mapping = new
            {
                prefix = @"?<prefix>[\[\(]",
                number = @"?<number>[0-9]+",
                suffix = @"?<suffix>[\]\)]"
            };

            string result = RegexFormat.Rewrite("(prefix?)(number)(suffix?)", mapping);

            Assert.AreEqual(@"(?<prefix>[\[\(]?)(?<number>[0-9]+)(?<suffix>[\]\)]?)", result);

            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex(result);
            System.Text.RegularExpressions.Match m = regex.Match(@"(123045)");
            Assert.IsTrue(m.Success);
            System.Text.RegularExpressions.Match m2 = regex.Match(@"[123045]");
            Assert.IsTrue(m2.Success);
            return;
        }
    }
}
