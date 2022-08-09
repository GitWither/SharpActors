using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpActors
{
    using Actor = Int32;

    public class ActorView : IEnumerable<Actor>
    {
        internal HashSet<Actor> Actors { get; }
        public int Count => Actors.Count;

        internal ActorView()
        {
            this.Actors = new HashSet<Actor>();
        }
        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)Actors).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
