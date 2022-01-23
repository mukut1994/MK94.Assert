using System;
using System.Collections.Generic;
using System.Text;

namespace MK94.Assert
{
    public class DifferenceException : Exception
    {
        public DifferenceException(string message, List<Difference> differences) : base(message)
        {
        }

        public List<Difference> Differences { get; set; }
    }
}
