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
        [Test]
        public void Should_be_comparable()
        {
            AllocatesString("lol", (template, allocated) =>
            {
                Check.That(allocated.Equals(template)).IsTrue();
                Check.That(allocated == template).IsTrue();
                Check.That(ReferenceEquals(allocated, template)).IsFalse();
            });
        }

        [Test]
        public void Should_support_encoding()
        {
            AllocatesString("foo", (template, allocated) => Check.That(allocated).Equals(template), Encoding.ASCII);
            AllocatesString("rŒma–––in", (template, allocated) => Check.That(allocated).Equals(template), Encoding.UTF8);
            AllocatesString("мясо", (template, allocated) => Check.That(allocated).Equals(template), Encoding.UTF32);
            AllocatesString("v̤̝̺̬͔ͅi̝a҉͚̱̪̻͎̝̮ṋ̀d̦͖̖̮͞e̘̗͎̹", (template, allocated) => Check.That(allocated).Equals(template), Encoding.Unicode);
        }

        [Test]
        public void Should_support_locking()
        {
            AllocatesString("romain", (template, allocated) =>
            {
                lock (allocated)
                {
                }
            });
        }

        [Test]
        public void Should_support_method_dispatch()
        {
            AllocatesString("lol", (template, allocated) =>
            {
                Check.That(allocated.ToString()).Equals(template.ToString());
                Check.That(allocated.ToLower()).Equals(template.ToLower());
                Check.That(allocated.ToUpper()).Equals(template.ToUpper());
            });
        }

        [Test]
        public void Should_support_pinning_and_writing()
        {
            AllocatesString("romain", (template, allocated) =>
            {
                unsafe
                {
                    fixed (char* c = allocated)
                    {
                        for (var i = 0; i < allocated.Length; i++)
                        {
                            c[i] = char.ToUpper(c[i]);
                        }

                        Check.That(allocated).Equals(template.ToUpper());
                    }
                }
            });
        }

        [Test]
        public void Should_support_reading()
        {
            AllocatesString("romain", (template, allocated) =>
            {
                for (var i = 0; i < allocated.Length; i++)
                {
                    Check.That(allocated[i]).Equals(template[i]);
                }
            });
        }

        [Test]
        public void Should_support_hashcode()
        {
            AllocatesString("lol", (template, allocated) =>
            {
                var dictionary = new Dictionary<string, int> {{allocated, 42}};
                Check.That(dictionary[allocated]).Equals(42);
                Check.That(allocated.GetHashCode()).Equals(template.GetHashCode());
            });
        }

        [Test]
        public void Should_support_synchronized_concurrent_access()
        {
            AllocatesString("romain", (template, allocated) =>
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

        private static void AllocatesString(string template, Action<string, string> assertion, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            var bytes = encoding.GetBytes(template);

            UnsafeStackAllocator.NewString(bytes, bytes.Length, encoding.GetDecoder(), x => assertion(template, x));
        }
    }
}