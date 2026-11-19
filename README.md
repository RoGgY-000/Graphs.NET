# Graphs.NET

[![NuGet](https://shields.io)](https://nuget.org)
[![License: MIT](https://shields.io)](LICENSE)
[![.NET](https://shields.io)](https://microsoft.com)

A high-performance, zero-allocation, Native AOT-ready graph library for **.NET 10+** built on Compressed Sparse Row (CSR) storage and custom bit-packed state primitives.

Designed for large-scale graphs (tens to hundreds of millions of vertices) where traditional object-oriented graph libraries suffer from heavy memory overhead and GC pressure.

> **Status:** Alpha (`v0.1.0-alpha`). The core CSR storage engine, bit-packed state primitives, and initial topological algorithms are complete and covered by unit tests (>95% line coverage) and BenchmarkDotNet suites.

---

## Key Features

* **Compressed Sparse Row (CSR) Storage:** Immutable `Graph` instances store topology in flat, contiguous arrays (`_offsets` and `_targets`), maximizing CPU cache locality and eliminating per-vertex/per-edge heap allocations.
* **Zero-Allocation Traversal:** Neighbor access returns `ReadOnlySpan<int>` directly over the underlying target buffer (`GetNeighbors(int vertex)`).
* **O(1) Arc Slot Recycling:** `GraphBuilder` uses an intrusive free-list over its internal edge buffer, reusing freed slots in `O(1)` time with zero extra allocations upon arc or vertex removal.
* **Bit-Packed State Primitives:** Custom `OneBitArray` and stack/pool-friendly `TwoBitArraySpan` (`ref struct`) pack vertex states into 1 or 2 bits per vertex instead of `bool[]` or `HashSet<int>`.
* **Iterative Algorithms (No Recursion):** Graph traversals rely on pooled stacks (`ArrayPool<int>.Shared`), preventing `StackOverflowException` on deep graphs with millions of vertices.
* **Native AOT Compatible:** Built without reflection or dynamic code generation (`<IsAotCompatible>true</IsAotCompatible>`).

---

## Installation

```bash
dotnet add package Graphs.NET --prerelease
```

---

## Quick Start

### 1. Building a Graph

Use `GraphBuilder` to construct a graph dynamically, then call `Build()` to pack it into a cache-friendly immutable `Graph`:

```csharp
using Graphs.NET;

var builder = new GraphBuilder(vertexCapacity: 5, arcCapacity: 6);

// Add directed arcs (automatically creates vertices if they do not exist yet)
builder.AddArcAndVertices(0, 1);
builder.AddArcAndVertices(0, 2);
builder.AddArcAndVertices(1, 3);
builder.AddArcAndVertices(2, 3);
builder.AddArcAndVertices(3, 4);

// Optional: filter self-loops and duplicate parallel arcs during Build()
var options = new GraphBuildingOptions()
    .DisableReflexiveEdges()
    .DisableDuplicateEdges()
    .Fix();

Graph graph = builder.Build(options);
```

### 2. Querying & Running Algorithms

```csharp
// Zero-allocation neighbor iteration via ReadOnlySpan<int> (O(1))
ReadOnlySpan<int> neighbors = graph.GetNeighbors(0);

// Fast adjacency lookup via binary search over sorted neighbors (O(log(deg(v))))
bool hasArc = graph.HasArc(0, 2);

// Lazy-evaluated, cached graph metrics
bool isAcyclic = graph.IsAcyclic;
int duplicates = graph.DuplicateArcCount;
int selfLoops = graph.ReflexiveArcCount;

// Zero-allocation topological sort into a caller-provided Span<int>
Span<int> sorted = stackalloc int[graph.VertexCount];
if (graph.TryTopologicalSort(sorted))
{
    Console.WriteLine(\$"Topological order: {string.Join(" -> ", sorted.ToArray())}");
}
```

---

## Architecture Overview

`Graphs.NET` separates **graph construction** from **graph querying and algorithms**:

1. **Construction Phase (`GraphBuilder`):**
   * Uses a forward-star array representation (`_tails`, `_targets`, `_nexts`) paired with a `OneBitArray` active-vertex mask.
   * Supports dynamic insertion and removal of vertices and arcs without object allocations per edge.
   * Freed arc indices are tracked via an intrusive free-list (`_freeListTail`) and reused immediately on subsequent insertions.
2. **Compilation Phase (`Build()`):**
   * Rents temporary scratch buffers from `ArrayPool<int>.Shared`.
   * Filters reflexive/duplicate arcs if requested, sorts each vertex's adjacency list for fast binary search, and packs the final topology into compact `_offsets` and `_targets` CSR arrays.
3. **Algorithm Phase (`Graph` & `GraphAlgorithmsExtensions`):**
   * Operates strictly over `ReadOnlySpan<int>` slices and `TwoBitArraySpan` bitmasks, achieving **0 B** managed heap allocations during execution.

---

## Benchmarks

*Benchmarks measured on .NET 10 using BenchmarkDotNet.*

| Method | Vertices | Arcs | Mean | Allocated |
| :--- | :--- | :--- | :--- | :--- |
| `GraphBuilder.Build` | 1,000,000 | 5,000,000 | *... ms* | *... MB* |
| `Graph.IsAcyclic` | 1,000,000 | 5,000,000 | *... ms* | **0 B** |
| `Graph.TryTopologicalSort` | 1,000,000 | 5,000,000 | *... ms* | **0 B** |

*(Update with your BenchmarkDotNet results)*

---

## Roadmap

- [x] CSR `Graph` representation & `GraphBuilder` with O(1) free-list slot reuse
- [x] Bit-packed `OneBitArray` and `TwoBitArraySpan` primitives
- [x] Iterative Cycle Detection (`IsAcyclic`) & `TryTopologicalSort`
- [ ] Breadth-First Search (BFS) & unweighted shortest paths
- [ ] Depth-First Search (DFS) & Connected Components
- [ ] Strongly Connected Components (Tarjan's algorithm)
- [ ] Bridges, Articulation Points & Bipartite graph detection
- [ ] Weighted graphs & zero-overhead Generic Math policies (`Dijkstra`, `A*`, `Prim`, `Kruskal`)

---

## Requirements

* **.NET 10.0** or later

## License

This project is licensed under the [MIT License](LICENSE).
