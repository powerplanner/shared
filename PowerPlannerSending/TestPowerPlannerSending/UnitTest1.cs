using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Runtime.Serialization.Json;
using PowerPlannerSending;
using System.Collections.Generic;

namespace TestPowerPlannerSending
{
    [TestClass]
    public class UnitTest1
    {
        public static readonly Type[] KnownTypes = new Type[]
            {
                typeof(Year),
                typeof(Teacher),

                typeof(Semester),

                typeof(Class),
                typeof(Task),

                typeof(Homework),
                typeof(Exam),
                typeof(Schedule),
                typeof(WeightCategory),

                typeof(Grade),
                typeof(TeacherUnderSchedule)
            };

        private static string serialized;

        [TestMethod]
        public void TestMethod1()
        {
            List<BaseItem> items = new List<BaseItem>();

            items.Add(new Homework() { Name = "Homework1", Date = DateTime.Today.AddSeconds(4) });
            items.Add(new Task() { Name = "Task1" });

            items = PassThrough(items);

            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(items[0] is Homework);
            Assert.IsTrue(items[1] is Task);

            Assert.AreEqual(4, (items[0] as Homework).Date.Second);
        }

        private static List<BaseItem> PassThrough(List<BaseItem> list)
        {
            return Deserialize<List<BaseItem>>(Serialize(list, KnownTypes), KnownTypes);
        }

        private static MemoryStream Serialize<T>(T obj, Type[] knownTypes)
        {
            MemoryStream stream = new MemoryStream();

            new DataContractJsonSerializer(typeof(T), knownTypes).WriteObject(stream, obj);

            stream.Position = 0;

            serialized = new StreamReader(stream).ReadToEnd();
            stream.Position = 0;
            return stream;
        }

        private static T Deserialize<T>(MemoryStream stream, Type[] knownTypes)
        {
            return (T)new DataContractJsonSerializer(typeof(T), knownTypes).ReadObject(stream);
        }
    }
}
