using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpActors
{
    using Actor = Int32;
    public class ActorSystem
    {
        public HashSet<Actor> Actors { get; set; }

        public ActorSystem()
        {
            Actors = new HashSet<int>();
        }
    }
}
