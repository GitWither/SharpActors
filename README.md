# SharpActors
A simple C# Entity Component System. This library aims to simplify and speed up the implementation of ECS in C# games and engines.

## Key Features
 - Actors (entities) are integers and hold no data
 - Components are simple structs and are stored in contiguous memory
 - A view system that allows you to query actors during runtime
 - A System implementation that allows you to process entities based on their components

## Usage
It's very easy to start using this library. Simply add it as a reference to your project. To start using it in code, you have to initialize an `ActorRegistry`:
```cs
const int maxActors = 10000;
ActorRegistry actorRegistry = new ActorRegistry(maxActors);
```
### Creating Actors
```cs
int actor = actorRegistry.CreateActor();
```

### Managing Components
```cs
struct CoolComponent {
  public int coolness;
}

actorRegistry.RegisterComponent<CoolComponent>();

actorRegistry.AddComponent<CoolComponent>(actor);
bool isCool = actorRegistry.HasComponent<CoolComponent>(actor);
actorRegistry.RemoveComponent<CoolComponent>(actor);
```

### Views
```cs
ActorView coolView = actorRegistry.CreateView<CoolComponent>();
Console.WriteLine(coolView.Count);
```

### Systems
A System can be any class that inherits from `ActorSystem`
```cs
public RenderSystem : ActorSystem {
  void CoolFunc() {
    //Access actors with //this.Actors
  }
}

RenderSystem renderSystem = actorRegistry.RegisterSystem<CoolComponent, RenderingComponent>()
```

You can find more examples in the [Unit Tests](https://github.com/GitWither/SharpActors/blob/master/SharpActorsTests/SharpActorsTests.cs)
