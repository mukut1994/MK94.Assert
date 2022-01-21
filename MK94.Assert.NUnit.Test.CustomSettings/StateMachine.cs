using System;

namespace MK94.Assert.NUnit.Test.CustomSettings
{
    public class StateMachine
    {
        public int A { get; set; }
        public int B { get; set; }

        public void SetStateA(int a)
        {
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
