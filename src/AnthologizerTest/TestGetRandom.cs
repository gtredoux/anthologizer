using System;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using com.renoster.Anthologizer.Media;

namespace com.renoster.Anthologizer.Impl
{
    [TestClass]
    public class TestGetRandom
    {
        [TestMethod]
        public void TestGetRandom1()
        {
            AnthologizerService svc = new AnthologizerService();
            Dictionary<string,bool> seen = new Dictionary<string, bool>();

            string context = "foobar";
            string root = @"c:\Music";

            for (int i = 0; i < 5; i++)
            {
                List<Item> result = svc.GetRandom(root, context, 5, "/");
                Assert.AreEqual(5, result.Count);
                CheckSeen(result, seen);
            }
        }

        private static void CheckSeen(List<Item> result, Dictionary<string, bool> seen)
        {
            foreach (Item item in result)
            {
                Assert.IsFalse(seen.ContainsKey(item.Id));
                seen.Add(item.Id, true);
            }
        }
    }
}
