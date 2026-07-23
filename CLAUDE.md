# Project AI Development Guidelines

## Role of Claude Code

Claude Code acts as the primary architect, planner, technical lead, and code reviewer.

For simple tasks:
1. Understand the existing code.
2. Make a concise plan.
3. Implement or delegate as appropriate.
4. Review the result.
5. Validate the implementation.

For medium and large tasks:
1. Inspect the existing architecture before making changes.
2. Identify dependencies and affected systems.
3. Think through the architecture and responsibilities.
4. Create an implementation plan before coding.
5. Prefer delegation of implementation work to OpenCode.
6. Review OpenCode's implementation.
7. Run validation and tests.
8. Fix architectural or quality issues before considering the task complete.

Do not rush into coding large features.

For complex features, think about:
- Responsibilities
- Dependencies
- Data flow
- Lifetime management
- Event flow
- Object ownership
- Extensibility
- Testability
- Performance
- Unity lifecycle implications

Prefer modifying the existing architecture consistently rather than introducing isolated solutions.

---

# Unity C# Coding Standards

## General Principles

Follow:
- SOLID principles
- Clean Code principles
- Separation of Concerns
- Single Responsibility Principle
- Dependency Inversion
- Composition over inheritance when appropriate
- Interface-driven design where appropriate
- Explicit dependencies
- Small focused classes
- Small focused methods
- Clear naming

Avoid:
- God classes
- God MonoBehaviours
- Large methods
- Deeply nested conditionals
- Hidden dependencies
- Static global state unless explicitly justified
- Unnecessary singletons
- Tight coupling between systems
- Duplicate logic
- Magic numbers
- Magic strings

Prefer readable and maintainable code over clever code.

---

# Architecture

When implementing a feature, identify:

1. Core domain logic
2. Presentation/UI logic
3. Infrastructure/platform logic
4. Data/configuration
5. External dependencies
6. Event communication
7. Lifecycle management

Keep systems decoupled.

Use interfaces when they provide a meaningful abstraction or enable:
- Dependency inversion
- Testability
- Multiple implementations
- Decoupling between systems

Do not create interfaces purely for the sake of having interfaces.

---

# Reactive Programming

Use R3 for reactive/event-driven behavior where appropriate.

Prefer reactive patterns over unnecessary polling and Update loops.

Avoid using Update() for:
- Polling state that can be observed reactively
- Repeated checks for events
- Timer logic
- State synchronization
- UI state updates
- Input/state observation when an event/reactive alternative exists

Prefer:
- R3 observables
- ReactiveProperty
- Subject
- Events
- Reactive subscriptions
- Appropriate lifecycle disposal

Use Update() only when it is genuinely the correct tool, such as:
- Per-frame simulation
- Physics-related continuous logic
- Direct frame-dependent calculations
- Systems that fundamentally require frame-by-frame evaluation

Do not force reactive programming into systems where Update() is the simpler and more appropriate solution.

Always consider subscription lifetime and disposal.

Avoid memory leaks and dangling subscriptions.

---

# Tweening

Use LitMotion for tweening and animation sequences.

Do not introduce DOTween or PrimeTween for new code unless explicitly requested.

Prefer LitMotion for:
- UI animations
- Transform animations
- Scale/position/rotation transitions
- Fade animations
- Value interpolation
- Sequenced animations

Keep animation logic separated from business logic where practical.

---

# Asynchronous Operations and Waiting

Use Unity Awaitable for asynchronous operations and time-based waits where appropriate.

Prefer:
- Awaitable
- async/await
- CancellationToken
- R3 async/reactive integrations where appropriate

Avoid:
- Coroutine-based waiting when Awaitable is a better fit
- Manual timer state machines
- Update-based timers
- Busy polling

For time delays, prefer Awaitable-based delays.

Always consider cancellation and object lifetime.

Do not continue async work after the owning object has been destroyed.

---

# Unity Lifecycle

Be careful with:
- Awake
- OnEnable
- Start
- OnDisable
- OnDestroy

Do not put excessive logic into Unity lifecycle methods.

Use lifecycle methods primarily for:
- Initialization
- Subscription
- Unsubscription
- Cleanup

Ensure subscriptions and resources are correctly disposed.

---

# Events and Communication

Prefer loose coupling between systems.

Use:
- Interfaces
- R3
- C# events
- Event buses/message systems when appropriate

Avoid direct references between unrelated systems when an abstraction can be used.

Do not create a global event bus for every communication problem.

Choose the simplest architecture that provides appropriate decoupling.

---

# Dependency Injection

Prefer explicit dependencies.

When appropriate, use:
- Constructor injection for pure C# classes
- Serialized references for Unity-authored dependencies
- Dependency injection frameworks when already present in the project

Avoid service locators unless the architecture explicitly requires one.

Avoid hidden dependencies.

A class should make its important dependencies obvious.

---

# Design Patterns

Use design patterns when they solve an actual architectural problem.

Potential patterns include:
- Strategy
- State
- Command
- Observer
- Factory
- Builder
- Adapter
- Facade
- Repository
- Dependency Injection

Do not use patterns merely to make code appear more sophisticated.

Prefer simple composition when it provides the same result.

---

# ScriptableObjects

Use ScriptableObjects for:
- Shared configuration
- Static game data
- Authoring data
- Designers' editable data
- Event channels when appropriate

Do not use ScriptableObjects as a replacement for every data structure.

Separate configuration/data from runtime state.

Avoid storing mutable runtime state in shared ScriptableObjects unless intentionally designed.

---

# UI

Separate:
- UI presentation
- UI state
- Business logic
- Data

UI components should not contain large amounts of gameplay or domain logic.

Prefer reusable UI components.

For UI animations, use LitMotion.

For reactive UI state, prefer R3.

---

# Performance

Consider performance when writing:
- Per-frame logic
- Reactive subscriptions
- Allocations
- LINQ in hot paths
- GetComponent calls
- Instantiate/Destroy
- String allocations
- Garbage collection

Do not prematurely optimize normal code.

Optimize when there is evidence or a clear hot path.

Prefer readable code first, then optimize measured bottlenecks.

---

# Code Quality

Before considering a feature complete:

- Remove dead code.
- Remove unused fields.
- Remove unnecessary comments.
- Avoid commented-out code.
- Check naming.
- Check responsibilities.
- Check dependencies.
- Check lifecycle cleanup.
- Check cancellation.
- Check subscription disposal.
- Check null handling.
- Check error handling.
- Check performance-sensitive paths.

Do not add comments that merely restate the code.

Comments should explain WHY, not WHAT.

---

# Existing Project Conventions

Before creating new code:

1. Inspect similar existing systems.
2. Follow existing naming conventions.
3. Follow existing folder structure.
4. Reuse existing abstractions.
5. Reuse existing utilities.
6. Avoid creating duplicate systems.

Consistency with the existing project is important.

---

# Delegation to OpenCode

Claude Code should delegate implementation-heavy tasks to OpenCode when appropriate.

Claude Code should remain responsible for:
- Architecture
- Planning
- Technical decisions
- Task decomposition
- Code review
- Integration review
- Final validation

OpenCode should primarily handle:
- Implementation
- Boilerplate
- Multi-file code changes
- Refactoring according to the approved plan
- Tests
- Mechanical code changes

When delegating a complex task, provide OpenCode with:
1. Context
2. Goal
3. Existing architecture
4. Files to inspect
5. Requirements
6. Constraints
7. Expected implementation approach
8. Validation requirements

Do not delegate an ambiguous task without first defining the intended architecture.

---

# Before Coding Large Features

For large features, Claude Code should first produce:

## Architecture
- Components
- Responsibilities
- Dependencies
- Data flow
- Event flow
- Lifecycle
- Integration points

## Implementation Plan
- Step 1
- Step 2
- Step 3
- ...

## Risks
- Technical risks
- Performance risks
- Lifecycle risks
- Integration risks

## Validation
- Tests
- Build validation
- Unity editor validation
- Runtime validation

Only after this should implementation begin.

---

# Definition of Done

A task is complete only when:

1. The implementation matches the intended architecture.
2. The code follows the project coding standards.
3. R3 is used where reactive programming is appropriate.
4. LitMotion is used for new tweening.
5. Awaitable is used for appropriate asynchronous waits.
6. Dependencies are appropriately abstracted.
7. SOLID and Clean Code principles are respected.
8. Resources and subscriptions are properly cleaned up.
9. No unnecessary Update polling was introduced.
10. The code compiles.
11. Relevant tests or validation have been performed.
12. The final implementation has been reviewed.




## Architecture-First Rule

When a task is large, complex, or affects multiple systems, do not immediately start writing code.

First:
1. Understand the requirements.
2. Inspect the existing architecture.
3. Identify affected systems.
4. Identify dependencies.
5. Determine ownership and responsibilities.
6. Design the data and event flow.
7. Define the implementation plan.
8. Identify risks and edge cases.

Then delegate implementation to OpenCode.

Claude Code is responsible for architectural decisions.
OpenCode is responsible for executing the implementation plan.

After OpenCode completes the task, Claude Code must review the implementation against the original architecture before considering the task complete.