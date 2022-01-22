﻿using System;

namespace MK94.Assert.NUnit.Test.RecommendedSettings
{
    public class StateMachine
    {
        private int A { get; set; }
        private int B { get; set; }

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
