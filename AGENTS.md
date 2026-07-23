# OpenCode Agent Instructions

You are the implementation agent for this Unity C# project.

Follow all rules defined in CLAUDE.md.

Your primary responsibility is implementation.

Before changing code:
1. Inspect the existing implementation.
2. Understand existing architecture.
3. Reuse existing systems where possible.
4. Do not introduce duplicate abstractions.

For large changes:
1. Understand the requested architecture.
2. Inspect all relevant files.
3. Implement incrementally.
4. Keep changes focused.
5. Validate compilation and behavior.

Use:
- R3 for reactive programming where appropriate.
- LitMotion for tweening.
- Unity Awaitable for asynchronous waiting where appropriate.
- Interfaces and dependency inversion when useful.
- SOLID principles.
- Clean Code principles.
- Appropriate design patterns.

Avoid:
- Unnecessary Update loops.
- Coroutine-based waiting when Awaitable is appropriate.
- New DOTween/PrimeTween code.
- Unnecessary singletons.
- God classes.
- Large monolithic MonoBehaviours.
- Tight coupling.
- Unnecessary abstractions.

Do not redesign unrelated parts of the project.

If you discover an architectural problem that blocks implementation:
1. Explain the problem.
2. Propose the smallest appropriate architectural change.
3. Implement only after the intended direction is clear.

After implementation:
- Review changed files.
- Check for compilation errors.
- Check for null/lifetime issues.
- Check subscription disposal.
- Check async cancellation.
- Check unnecessary allocations.
- Report what was changed.