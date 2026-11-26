# C# Project Instructions

## Communication
- Chat answers: use the language of the prompt (usually German). This rule applies to chat responses only, not to generated artifacts.
- Start directly with the answer. No preamble, no praise, no restating the task.
- Be compact and direct. No filler sentences. Use tables where they improve overview; use bullet lists only for real enumerations.
- In prose, use commas, parentheses or separate sentences for asides. Never use en dashes, em dashes or hyphens as punctuation.
- The developer is a senior engineer. Do not explain basic C#/.NET concepts.
- Domain terms from public transport, rail, railway engineering and software architecture (e.g. DS100/RIL100, GTFS, NeTEx, Transmodel, railML, IVU, OSM railway tagging) are known. Never explain them.

## Commit Messages
- Always write commit messages in English, regardless of the prompt language, IDE language or existing commit history.
- Summary line in imperative mood, max. 72 characters, no trailing period.
- Optional body after a blank line, explaining why the change was made.
- PR titles and PR descriptions are also written in English.

## Language Conventions
- All code comments, XML docs, log messages, exception messages and UI strings are written in English.
- Identifiers are English.

## Code Structure
- One type per file. File name matches the type name.
- No nested types (classes, records, structs or enums inside other classes). Move them into their own files.
- Use file-scoped namespaces.
- Nullable reference types are enabled. Respect annotations; avoid the null-forgiving operator `!` unless justified with a comment.
- Prefer `sealed` for classes not designed for inheritance.
- Prefer `record` / `readonly record struct` for immutable data carriers.

## LINQ & Collections
- Prefer `ToArray()` over `ToList()` unless the collection is modified afterwards.
- Avoid multiple enumeration of `IEnumerable<T>`; materialize once when needed.
- Do not rely on enumeration order of `HashSet<T>` or `Dictionary<TKey, TValue>` for results. Algorithms (routing, graph traversal, layout) must be deterministic; sort explicitly or use ordered structures.

## Equality & Hashing
- Custom structs used as keys must implement `IEquatable<T>` with consistent `Equals` and `GetHashCode`. Prefer `readonly record struct` where possible.
- Use `HashCode.Combine` for hash codes.

## Performance
- Use `[GeneratedRegex]` (source-generated regex) instead of constructing `Regex` instances at runtime or in loops.
- Avoid unnecessary allocations in hot paths (geometry, graph and routing code).
- Report long-running progress via `IProgress<T>`; accept `CancellationToken` in long-running and async operations.

## Geometry & Geodata
- Be explicit about coordinate reference systems (WGS84 vs. projected/local). Name variables and parameters accordingly.
- Projection roundtrips must be lossless within a defined tolerance; add tests for roundtrip drift when touching projection code.

## Dependencies & Tooling
- Code must build with Visual Studio 2022. Do not use language or SDK features that require a newer IDE unless the project file explicitly targets them.
- Do not add NuGet dependencies without stating why. Prefer BCL functionality.
- Keep warnings at zero; do not suppress analyzer warnings without a justifying comment.

## Testing
- Test framework: xUnit.
- Test names follow `Method_Scenario_ExpectedResult`.
- Cover edge cases (empty input, boundaries, cycles in graphs, date/time boundaries such as DST changes and midnight crossings).

## Changes & Reviews
- When modifying existing code, show only the changed parts with enough context to locate them, unless the full file is requested.
- Point out technical debt, potential bugs and simplifications made, explicitly and briefly.
