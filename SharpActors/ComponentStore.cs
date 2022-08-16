using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpActors
{
    using Actor = Int32;
    internal class ComponentStore<T> : IComponentStore where T : struct
    {
        private readonly T[] m_ComponentArray;
        private int m_Size;

        private readonly Dictionary<Actor, int> m_ActorToIndex;
        private readonly Dictionary<int, Actor> m_IndexToActor;

        public ComponentStore(int maxEntities)
        {
            this.m_ComponentArray = new T[maxEntities];
            this.m_ActorToIndex = new Dictionary<Actor, int>();
            this.m_IndexToActor = new Dictionary<int, Actor>();
        }

        public void InsertData(Actor actor, T component)
        {
            int newIndex = m_Size;
            m_ActorToIndex.Add(actor, newIndex);
            m_IndexToActor.Add(newIndex, actor);
            m_ComponentArray[newIndex] = component;
            m_Size++;
        }

        public ref T GetData(Actor actor)
        {
            return ref m_ComponentArray[m_ActorToIndex[actor]];
        }

        public void RemoveData(Actor actor)
        {
            int removedActor = m_ActorToIndex[actor];
            int lastElement = m_Size - 1;
            m_ComponentArray[removedActor] = m_ComponentArray[lastElement];

            Actor lastElementActor = m_IndexToActor[lastElement];
            m_ActorToIndex[lastElementActor] = removedActor;
            m_IndexToActor[removedActor] = lastElementActor;

            m_IndexToActor.Remove(lastElement);
            m_ActorToIndex.Remove(actor);

            m_Size--;
        }

        public void CleanUpActor(Actor actor)
        {
            if (!m_ActorToIndex.ContainsKey(actor)) return;

            this.RemoveData(actor);
        }
    }
}
