# Development Log
> Agentic hindsight - reverse chronological
---

## 2026-01-08: DOOM FPS Enemy Navigation System

**Changes**: Research/documentation only - no code changes

**Learnings**:
- Enemy movement controlled by `EnemyMobile.cs` (AI state machine) + `EnemyController.cs` (NavMeshAgent wrapper)
- AI states: Patrol → Follow → Attack, transitions in `UpdateAIStateTransitions()`
- Chase logic sets NavMeshAgent destination to player position in `AIState.Follow`
- Speed configured via `NavigationModule` component, applied in `EnemyController.Start():149-155`
- Detection handled by `DetectionModule.cs` with raycasts and range checks

**Hindsight**:
- Unity 6 deprecated the built-in Navigation window - use **AI Navigation package** instead
- NavMeshSurface component replaces global bake settings
- To mark floors walkable: put on specific Layer → set NavMeshSurface "Include Layers" → Bake
- NavMeshModifier component excludes objects or sets area types
- NavMeshAgent on enemies unchanged - only baking workflow changed

**Context**:
- `Assets/DOOM/FPS/Scripts/EnemyMobile.cs` - AI state machine
- `Assets/DOOM/FPS/Scripts/EnemyController.cs` - movement execution
- `Assets/DOOM/FPS/Scripts/DetectionModule.cs` - player detection
- `Assets/DOOM/FPS/Scripts/NavigationModule.cs` - speed parameters
