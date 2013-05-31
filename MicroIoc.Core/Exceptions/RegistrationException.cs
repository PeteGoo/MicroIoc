using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroIoc
{
    public class RegistrationException : Exception
    {
        public RegistrationException(string message)
            : base(message) { }
    }
}
