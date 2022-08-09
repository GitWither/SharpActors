using System.Diagnostics;

namespace SharpActors
{
    using Actor = Int32;
    using Mask = Int32;

    public class ActorRegistry
    {
        private readonly Queue<Actor> m_AvailableActors;
        private readonly int m_MaxActors;
        private int m_AliveActors = 0;
        private readonly Mask[] m_ComponentMask;

        private int m_NextComponentId = 0;
        private readonly Dictionary<Type, int> m_ComponentIds;
        private readonly Dictionary<Type, IComponentStore> m_ComponentStores;

        private readonly Dictionary<Type, ActorSystem> m_Systems;
        private readonly Dictionary<Type, Mask> m_SystemMask;

        public int Count => m_AliveActors;
        public int RegisteredComponents => m_ComponentIds.Count;

        public ActorRegistry(int maxActors)
        {
            m_MaxActors = maxActors;

            m_Systems = new Dictionary<Type, ActorSystem>();
            m_SystemMask = new Dictionary<Type, Mask>();

            m_ComponentIds = new Dictionary<Type, int>();
            m_ComponentStores = new Dictionary<Type, IComponentStore>();
            m_ComponentMask = new int[maxActors];
            m_AvailableActors = new Queue<Actor>(maxActors);
            for (int i = 0; i < maxActors; i++)
            {
                m_AvailableActors.Enqueue(i);
            }
        }

        #region Systems
        public T RegisterSystem<T, C1>() 
            where T : ActorSystem, new()
            where C1 : struct
        {
            Mask mask = 0;
            mask |= (1 << m_ComponentIds[typeof(C1)]);


            return RegisterSystem<T>(mask);
        }

        public T RegisterSystem<T, C1, C2>()
            where T : ActorSystem, new()
            where C1 : struct
            where C2 : struct
        {
            Mask mask = 0;
            mask |= (1 << m_ComponentIds[typeof(C1)]);
            mask |= (1 << m_ComponentIds[typeof(C2)]);


            return RegisterSystem<T>(mask);
        }

        public T RegisterSystem<T, C1, C2, C3>()
            where T : ActorSystem, new()
            where C1 : struct
            where C2 : struct
        {
            Mask mask = 0;
            mask |= (1 << m_ComponentIds[typeof(C1)]);
            mask |= (1 << m_ComponentIds[typeof(C2)]);
            mask |= (1 << m_ComponentIds[typeof(C3)]);


            return RegisterSystem<T>(mask);
        }

        private T RegisterSystem<T>(Mask mask) where T : ActorSystem, new()
        {
            T system = new();
            m_Systems.Add(typeof(T), system);
            m_SystemMask.Add(typeof(T), mask);
            return system;
        }

        private void UpdateSystems(Actor actor, int actorMask)
        {
            foreach (KeyValuePair<Type, ActorSystem> pair in m_Systems)
            {
                int systemMask = m_SystemMask[pair.Value.GetType()];
                if ((actorMask & systemMask) == systemMask)
                {
                    pair.Value.Actors.Add(actor);
                }
                else
                {
                    pair.Value.Actors.Remove(actor);
                }
            }
        }
        #endregion

        #region Actor

        public Actor CreateActor()
        {
            Actor actor = m_AvailableActors.Dequeue();
            m_AliveActors++;

            return actor;
        }

        public void DestroyActor(Actor actor)
        {
            m_AvailableActors.Enqueue(actor);
            m_ComponentMask[actor] = 0;

            foreach (IComponentStore store in m_ComponentStores.Values)
            {
                store.CleanUpActor(actor);
            }

            foreach (KeyValuePair<Type, ActorSystem> pair in m_Systems)
            {
                pair.Value.Actors.Remove(actor);
            }

            m_AliveActors--;
        }

        #endregion

        #region Components
        public void RegisterComponent<T>() where T : struct
        {
            m_ComponentStores.Add(typeof(T), new ComponentStore<T>(m_MaxActors));
            m_ComponentIds.Add(typeof(T), m_NextComponentId);
            m_NextComponentId++;
        }
        public ref T AddComponent<T>(Actor actor) where T : struct
        {
            return ref AddComponent(actor, new T());
        }

        public ref T AddComponent<T>(Actor actor, T component) where T : struct
        {
            Debug.Assert(m_ComponentIds.ContainsKey(typeof(T)), "Adding unregistered component to actor! Make sure to register a component before using it.");

            m_ComponentMask[actor] |= (1 << m_ComponentIds[typeof(T)]);

            ComponentStore<T> componentStore = (m_ComponentStores[typeof(T)] as ComponentStore<T>)!;

            Debug.Assert(componentStore != null, $"Component store not found for {typeof(T).Name}");

            componentStore.InsertData(actor, component);

            UpdateSystems(actor, m_ComponentMask[actor]);

            return ref componentStore.GetData(actor);
        }

        public ref T GetComponent<T>(Actor actor) where T : struct
        {
            Debug.Assert(m_ComponentIds.ContainsKey(typeof(T)), "Getting unregistered component to actor! Make sure to register a component before using it.");
            Debug.Assert(HasComponent<T>(actor), $"Actor doesn't have requested component: {typeof(T).Name}");

            ComponentStore<T> componentStore = (m_ComponentStores[typeof(T)] as ComponentStore<T>)!;

            Debug.Assert(componentStore != null, $"Component store not found for {typeof(T).Name}");

            return ref componentStore.GetData(actor);
        }


        public bool HasComponent<T>(Actor actor) where T : struct
        {
            return (m_ComponentMask[actor] & (1 << m_ComponentIds[typeof(T)])) != 0;
        }

        public void RemoveComponent<T>(Actor actor) where T : struct
        {
            Debug.Assert(HasComponent<T>(actor), $"Actor doesn't have requested component: {typeof(T).Name}");

            ComponentStore<T> componentStore = (m_ComponentStores[typeof(T)] as ComponentStore<T>)!;

            Debug.Assert(componentStore != null, $"Component store not found for {typeof(T).Name}");

            componentStore.RemoveData(actor);

            m_ComponentMask[actor] &= ~(1 << m_ComponentIds[typeof(T)]);

            UpdateSystems(actor, m_ComponentMask[actor]);
        }

        public int GetActorComponentCount(Actor actor)
        {
            int mask = m_ComponentMask[actor];
            int count = 0;

            while (mask != 0)
            {
                if ((mask & 1) != 0) count++;
                mask >>= 1;
            }

            return count;
        }
        #endregion

        #region Actor Views

        private ActorView CreateView(Mask mask)
        {
            ActorView result = new ActorView();

            for (int i = 0; i < m_AliveActors; i++)
            {
                if ((m_ComponentMask[i] & mask) == mask)
                {
                    result.Actors.Add(i);
                }
            }

            return result;
        }

        public ActorView CreateView<C1>() where C1 : struct
        {
            int mask = 0;
            mask |= 1 << m_ComponentIds[typeof(C1)];

            return CreateView(mask);
        }

        public ActorView CreateView<C1, C2>() 
            where C1 : struct
            where C2 : struct
        {
            int mask = 0;

            mask |= 1 << m_ComponentIds[typeof(C1)];
            mask |= 1 << m_ComponentIds[typeof(C2)];

            return CreateView(mask);
        }

        public ActorView CreateView<C1, C2, C3>()
            where C1 : struct
            where C2 : struct
            where C3 : struct
        {
            int mask = 0;

            mask |= 1 << m_ComponentIds[typeof(C1)];
            mask |= 1 << m_ComponentIds[typeof(C2)];
            mask |= 1 << m_ComponentIds[typeof(C3)];

            return CreateView(mask);
        }

        #endregion
    }
}