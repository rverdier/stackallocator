using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;

namespace StackAllocator.Tests
{
    [TestFixture]
    public class StackAllocatedArrayTests
    {
        [Test]
        public void Should_be_sized_properly()
        {
            AllocatesArray(new byte[42], (template, allocated) =>
            {
                Check.That(allocated.Length).Equals(template.Length);
                Check.That(allocated.LongLength).Equals(template.LongLength);
            });
        }

        [Test]
        public void Should_support_blittable_type_arrays()
        {
            CheckBlittableArray<byte>();
            CheckBlittableArray<sbyte>();
            CheckBlittableArray<short>();
            CheckBlittableArray<int>();
            CheckBlittableArray<uint>();
            CheckBlittableArray<long>();
            CheckBlittableArray<ulong>();
            CheckBlittableArray<IntPtr>();
            CheckBlittableArray<float>();
            CheckBlittableArray<double>();

            CheckBlittableArray<char>();
            CheckBlittableArray<TestIntEnum>();
            CheckBlittableArray<TestShortEnum>();
            CheckBlittableArray<TestStruct>();
        }

        [Test]
        public void Should_support_hashcode()
        {
            AllocatesArray(new char[42], (template, allocated) =>
            {
                var dictionary = new Dictionary<char[], int> {{allocated, 42}};
                Check.That(dictionary[allocated]).Equals(42);
            });
        }

        [Test]
        public void Should_support_locking()
        {
            AllocatesArray(new byte[42], (template, allocated) =>
            {
                lock (allocated)
                {
                }
            });
        }

        [Test]
        public void Should_support_method_dispatch()
        {
            AllocatesArray(new byte[42], (template, allocated) =>
            {
                Check.That(allocated.ToString()).Equals(template.ToString());
                Check.That(allocated.GetLowerBound(0)).Equals(template.GetLowerBound(0));
            });
        }

        [Test]
        public void Should_support_synchronized_concurrent_access()
        {
            AllocatesArray(new int[1], (template, allocated) =>
            {
                const int iterationCount = 10*1000*1000;
                var t1 = Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < iterationCount; i++)
                    {
                        lock (allocated)
                            allocated[0]++;
                    }
                });

                var t2 = Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < iterationCount; i++)
                    {
                        lock (allocated)
                            allocated[0]++;
                    }
                });

                Task.WaitAll(t1, t2);
                Check.That(allocated[0]).IsEqualTo(iterationCount*2);
            });
        }

        [Test]
        public void Should_support_writing_and_reading()
        {
            AllocatesArray(new char[42], (template, allocated) =>
            {
                for (var i = 0; i < allocated.Length; i++)
                {
                    allocated[i] = char.ToUpper(allocated[i]);
                }

                for (var i = 0; i < allocated.Length; i++)
                {
                    Check.That(allocated[i]).Equals(char.ToUpper(template[i]));
                }
            });
        }

        private static void CheckBlittableArray<T>()
            where T : struct
        {
            AllocatesArray(new T[42], (template, allocated) =>
            {
                Check.That(allocated.Length).Equals(template.Length);
                Check.That(allocated.ToString()).Equals(template.ToString());
            });
        }

        private static void AllocatesArray<T>(T[] template, Action<T[], T[]> assertion) where T : struct
        {
            UnsafeStackAllocator.NewArrayOf<T>(template.Length, allocated => assertion(template, allocated));
        }

        private enum TestIntEnum : short
        {
            A,
            B,
            C
        }

        private enum TestShortEnum
        {
            A,
            B,
            C
        }

        private struct TestStruct
        {
            public int Value;
        }
    }
}