using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpActors
{
    internal interface IComponentStore
    {
        void CleanUpActor(int actor);
    }
}
