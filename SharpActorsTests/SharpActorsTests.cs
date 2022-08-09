using SharpActors;

namespace SharpActorsTests
{
    public struct TransformComponent
    {
        public float x, y, z;
    }

    public struct SpriteRendererComponent
    {
        public int textureHandle;
        public int color;
    }

    public struct MeshComponent
    {
        public int handle;
    }

    public struct RigidbodyComponent
    {
        public int handle;
    }

    public class RenderSystem : ActorSystem
    {
        void Update()
        {
            foreach (int actor in this.Actors)
            {
                
            }
        }
    }

    public class PhysicsSystem : ActorSystem
    {

    }


    [TestClass]
    public class SharpActorsTests
    {
        public const int MaxActors = 150000;

        [TestMethod]
        public void TestCreation()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);

            Assert.AreEqual(0, actorRegistry.Count);
            Assert.IsNotNull(actorRegistry);
        }

        [TestMethod]
        public void TestEntityAdding()
        {
            const int actorsToAdd = 15000;

            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);

            for (int i = 0; i < actorsToAdd; i++)
            {
                actorRegistry.CreateActor();
            }

            Assert.AreEqual(actorsToAdd, actorRegistry.Count);
        }

        [TestMethod]
        public void TestComponentRegistration()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);
            actorRegistry.RegisterComponent<TransformComponent>();
            actorRegistry.RegisterComponent<SpriteRendererComponent>();

            Assert.AreEqual(2, actorRegistry.RegisteredComponents);
        }

        [TestMethod]
        public void TestComponentAddingRemoving()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);
            actorRegistry.RegisterComponent<TransformComponent>();
            actorRegistry.RegisterComponent<SpriteRendererComponent>();

            int actor = actorRegistry.CreateActor();

            actorRegistry.AddComponent<SpriteRendererComponent>(actor);
            actorRegistry.AddComponent<TransformComponent>(actor);

            Assert.AreEqual(2, actorRegistry.GetActorComponentCount(actor));

            actorRegistry.RemoveComponent<TransformComponent>(actor);

            Assert.AreEqual(1, actorRegistry.GetActorComponentCount(actor));

            Assert.IsTrue(actorRegistry.HasComponent<SpriteRendererComponent>(actor));

            actorRegistry.RemoveComponent<SpriteRendererComponent>(actor);

            Assert.AreEqual(0, actorRegistry.GetActorComponentCount(actor));
            Assert.IsFalse(actorRegistry.HasComponent<TransformComponent>(actor));
        }

        [TestMethod]
        public void TestActorIdOrder()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);

            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(i, actorRegistry.CreateActor());
            }
        }

        [TestMethod]
        public void TestMassComponentAddingRemoving()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);

            actorRegistry.RegisterComponent<TransformComponent>();
            actorRegistry.RegisterComponent<SpriteRendererComponent>();

            for (int i = 0; i < 145674; i++)
            {
                int actor = actorRegistry.CreateActor();
                actorRegistry.AddComponent<TransformComponent>(actor);
                actorRegistry.AddComponent<SpriteRendererComponent>(actor);
            }

            ref TransformComponent transform = ref actorRegistry.GetComponent<TransformComponent>(532);
            transform.x += 5;

            Assert.AreEqual(5, actorRegistry.GetComponent<TransformComponent>(532).x);
        }

        [TestMethod]
        public void TestActorViews()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);

            actorRegistry.RegisterComponent<TransformComponent>();
            actorRegistry.RegisterComponent<SpriteRendererComponent>();
            actorRegistry.RegisterComponent<MeshComponent>();

            for (int i = 0; i < 80000; i++)
            {
                int actor = actorRegistry.CreateActor();

                actorRegistry.AddComponent<MeshComponent>(actor);
                if (i % 2 == 0)
                {
                    actorRegistry.AddComponent<TransformComponent>(actor);
                }
                else
                {
                    actorRegistry.AddComponent<SpriteRendererComponent>(actor);
                }
            }

            ActorView transformView = actorRegistry.CreateView<TransformComponent>();
            Assert.AreEqual(40000, transformView.Count);

            ActorView spriteView = actorRegistry.CreateView<SpriteRendererComponent>();
            Assert.AreEqual(40000, spriteView.Count);

            ActorView bothView = actorRegistry.CreateView<TransformComponent, SpriteRendererComponent>();
            Assert.AreEqual(0, bothView.Count);

            ActorView meshView = actorRegistry.CreateView<MeshComponent>();
            Assert.AreEqual(80000, meshView.Count);
        }

        [TestMethod]
        public void TestCreateAndDestroy()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);

            for (int i = 0; i < 80000; i++)
            {
                int actor = actorRegistry.CreateActor();
            }

            for (int i = 0; i < 40000; i++)
            {
                actorRegistry.DestroyActor(i);
            }

            Assert.AreEqual(40000, actorRegistry.Count);
        }

        [TestMethod]
        public void TestDestroyedActorComponents()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);

            actorRegistry.RegisterComponent<MeshComponent>();

            for (int i = 0; i < 80000; i++)
            {
                int actor = actorRegistry.CreateActor();
                actorRegistry.AddComponent<MeshComponent>(actor);

                actorRegistry.DestroyActor(actor);

                Assert.IsFalse(actorRegistry.HasComponent<MeshComponent>(actor));
            }

            Assert.AreEqual(0, actorRegistry.Count);
        }


        [TestMethod]
        public void TestSystems()
        {
            ActorRegistry actorRegistry = new ActorRegistry(MaxActors);

            actorRegistry.RegisterComponent<TransformComponent>();
            actorRegistry.RegisterComponent<RigidbodyComponent>();
            actorRegistry.RegisterComponent<MeshComponent>();

            RenderSystem renderSystem = actorRegistry.RegisterSystem<RenderSystem, MeshComponent>();
            PhysicsSystem physicsSystem = actorRegistry.RegisterSystem<PhysicsSystem, TransformComponent, RigidbodyComponent>();

            for (int i = 0; i < 200; i++)
            {
                int actor = actorRegistry.CreateActor();
                actorRegistry.AddComponent<TransformComponent>(i);
                if (i > 150)
                {
                    actorRegistry.AddComponent<RigidbodyComponent>(i);
                }
                if (i > 100)
                {
                    actorRegistry.AddComponent<MeshComponent>(actor);
                }
            }

            Assert.AreEqual(99, renderSystem.Actors.Count);
            Assert.AreEqual(49, physicsSystem.Actors.Count);

            for (int i = 101; i < 200; i++)
            {
                actorRegistry.RemoveComponent<MeshComponent>(i);
            }

            Assert.AreEqual(49, physicsSystem.Actors.Count);
            Assert.AreEqual(0, renderSystem.Actors.Count);
        }
    }
}