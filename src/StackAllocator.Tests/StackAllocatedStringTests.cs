using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;

namespace StackAllocator.Tests
{
    [TestFixture]
    public class StackAllocatedStringTests
    {
        private static void AllocatesString(string originalString, Action<string, string> assertion, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            var bytes = encoding.GetBytes(originalString);

            UnsafeStackAllocator.NewString(bytes, bytes.Length, encoding.GetDecoder(), x => assertion(originalString, x));
        }

        [Test]
        public void Should_be_comparable()
        {
            AllocatesString("lol", (orignal, allocated) =>
            {
                Check.That(allocated.Equals(orignal)).IsTrue();
                Check.That(allocated == orignal).IsTrue();
                Check.That(ReferenceEquals(allocated, orignal)).IsFalse();
            });
        }

        [Test]
        public void Should_support_encoding()
        {
            AllocatesString("foo", (orignal, allocated) => Check.That(allocated).Equals(orignal), Encoding.ASCII);
            AllocatesString("rŒma–––in", (orignal, allocated) => Check.That(allocated).Equals(orignal), Encoding.UTF8);
            AllocatesString("мясо", (orignal, allocated) => Check.That(allocated).Equals(orignal), Encoding.UTF32);
            AllocatesString("v̤̝̺̬͔ͅi̝a҉͚̱̪̻͎̝̮ṋ̀d̦͖̖̮͞e̘̗͎̹", (orignal, allocated) => Check.That(allocated).Equals(orignal), Encoding.Unicode);
        }

        [Test]
        public void Should_support_locking()
        {
            AllocatesString("romain", (orignal, allocated) =>
            {
                lock (allocated)
                {
                }
            });
        }

        [Test]
        public void Should_support_method_dispatch()
        {
            AllocatesString("lol", (orignal, allocated) =>
            {
                Check.That(allocated.ToString()).Equals(orignal.ToString());
                Check.That(allocated.ToLower()).Equals(orignal.ToLower());
                Check.That(allocated.ToUpper()).Equals(orignal.ToUpper());
            });
        }

        [Test]
        public void Should_support_pinning_and_writing()
        {
            AllocatesString("romain", (orignal, allocated) =>
            {
                unsafe
                {
                    fixed (char* c = allocated)
                    {
                        for (var i = 0; i < allocated.Length; i++)
                        {
                            c[i] = char.ToUpper(c[i]);
                        }

                        Check.That(allocated).Equals(orignal.ToUpper());
                    }
                }
            });
        }

        [Test]
        public void Should_support_reading()
        {
            AllocatesString("romain", (orignal, allocated) =>
            {
                for (var i = 0; i < allocated.Length; i++)
                {
                    Check.That(allocated[i]).Equals(orignal[i]);
                }
            });
        }

        [Test]
        public void Should_support_string_hashcode()
        {
            AllocatesString("lol", (orignal, allocated) =>
            {
                var dictionary = new Dictionary<string, int> {{allocated, 42}};
                Check.That(dictionary[allocated]).Equals(42);
                Check.That(allocated.GetHashCode()).Equals(orignal.GetHashCode());
            });
        }

        [Test]
        public void Should_support_synchronized_concurrent_access()
        {
            AllocatesString("romain", (orignal, allocated) =>
            {
                var counter = 0;
                const int iterationCount = 10*1000*1000;
                var t1 = Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < iterationCount; i++)
                    {
                        lock (allocated)
                            counter++;
                    }
                });

                var t2 = Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < iterationCount; i++)
                    {
                        lock (allocated)
                            counter++;
                    }
                });

                Task.WaitAll(t1, t2);
                Check.That(counter).IsEqualTo(iterationCount*2);
            });
        }
    }
}