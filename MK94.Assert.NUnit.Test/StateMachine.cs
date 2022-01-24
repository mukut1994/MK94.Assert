using System;

namespace MK94.Assert.NUnit.Test
{
    public class StateMachine
    {
        public int A { get; set; }
        public int B { get; set; }

        public void SetStateA(int a)
        {
            if (a < 0)
                throw new ArgumentException("a < 0");

            A = a;
        }

        public void SetStateB(int b)
        {
            B = b;
        }

        public int Sum()
        {
            return A + B;
        }
    }
}
