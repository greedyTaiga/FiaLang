using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fia
{
    internal class Return : Exception
    {
        public readonly object? val;

        public Return(object? val)
        {   
            this.val = val;
        }
    }
}
