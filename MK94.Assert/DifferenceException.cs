using System;
using System.Collections.Generic;

namespace MK94.Assert
{
    public class DifferenceException : Exception
    {
        public DifferenceException(string message, List<Difference> differences) : base(message)
        {
            Differences = differences;
        }

        public List<Difference> Differences { get; set; }
    }
}
