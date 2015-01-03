using System;
using System.Text;
using NFluent;
using NUnit.Framework;

namespace StackAllocator.Tests
{
    [TestFixture]
    public class StackAllocatorTests
    {
        [Test]
        public void Should_allocates_string()
        {
            var encoding = Encoding.UTF8;
            var bytes = Encoding.UTF8.GetBytes("lol");
            UnsafeStackAllocator.NewString(bytes, bytes.Length, encoding.GetDecoder(), s =>
            {
                Check.That(s.Length).Equals(3);
                Check.That(s).Equals("lol");
            });
        }

        [Test]
        public void Should_allocates_array()
        {
            const int length = 42;

            UnsafeStackAllocator.NewArrayOf<int>(length, a =>
            {
                Check.That(a.Length).Equals(length);

                for (var i = 0; i < length; i++)
                {
                    a[i] = i;
                }

                for (var i = 0; i < length; i++)
                {
                    Check.That(a[i]).Equals(i);
                }
            });
        }
    }
}