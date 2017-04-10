using System;
using Xunit;

namespace bookings.core.tests
{
    public class MyClassShould
    {
        [Fact]
        public void AddCorrectly()
        {
            var sut = new MyClass();
            var result = sut.Add(1, 1);

            Assert.Equal(2, result);
        }
    }
}
