# Graphs.NET

A high-performance, zero-allocation, Native AOT-ready graph library for **.NET 10+** built on graph-specialized structures like Compressed Sparse Row (CSR) and forward star.

Designed for large-scale graphs (tens to hundreds of millions of vertices) where traditional object-oriented graph libraries suffer from heavy memory overhead and GC pressure.

> **Status:** Alpha (`v0.1.0-alpha`). The core CSR storage engine, bit-packed state primitives, and initial algorithms are complete and covered by unit tests (>95% line coverage) and BenchmarkDotNet suites.

---

## Key Features

* **Compressed Sparse Row (CSR) Storage:** Immutable `Graph` instances store topology in flat, contiguous arrays (`_offsets` and `_targets`), maximizing CPU cache locality and eliminating per-vertex/per-edge heap allocations.
* **Zero-Allocation Traversal:** Neighbor access returns sorted `ReadOnlySpan<int>` directly over the underlying target buffer (`GetNeighbors(int vertex)`).
* **O(1) Arc Slot Recycling:** `GraphBuilder` uses an intrusive free-list over its internal edge buffer, reusing freed slots in `O(1)` time with zero extra allocations upon arc or vertex removal.
* **Bit-Packed State Primitives:** Custom `OneBitArray` and  `TwoBitArraySpan` (`ref struct`) pack vertex states into 1 or 2 bits per vertex instead of `bool[]` or `HashSet<int>`.
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

*Benchmarks measured on .NET 10.0.13 (X64 RyuJIT AVX2) using BenchmarkDotNet v0.15.8 against a standard `Dictionary<int, List<int>>` (`Naive`) baseline and `QuikGraph`.*

### 1. Extreme Scale: Adding 100 Million Arcs

| Vertex Capacity | Arcs | Naive (`Dictionary` + `List`) | `Graphs.NET` (`GraphBuilder`) | Speedup |
| :--- | :--- | :--- | :--- | :--- |
| 1,000,000 | 1,000,000 | 305.99 ms | **18.07 ms** | **16.9x faster** |
| 1,000,000 | 10,000,000 | 2,018.00 ms | **121.21 ms** | **16.6x faster** |
| 1,000,000 | 100,000,000 | 16,100.00 ms | **1,113.00 ms** | **14.5x faster** |
| 10,000,000 | 10,000,000 | 5,576.00 ms | **291.13 ms** | **19.2x faster** |
| 10,000,000 | 100,000,000 | 42,788.00 ms | **2,313.00 ms** | **18.5x faster** |
| 100,000,000 | 100,000,000 | 80,760.00 ms *(1.346 m)* | **4,348.00 ms** | **18.6x faster** |

---

### 2. Graph Construction (`GraphBuilder.Build` vs `QuikGraph`)

Comparing packing a populated builder into an immutable Compressed Sparse Row (`Graph`) vs `QuikGraph`'s `ToCompressedSparseRowGraph()` and `ToArrayAdjacencyGraph()`:

| Vertices | Arcs | `QuikGraph` (ArrayAdjacency) | `QuikGraph` (CSR) | `Graphs.NET` (`Build`) | Speedup vs QuikGraph CSR |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1,000 | 1,000 | 20.31 μs | 18.16 μs | **3.69 μs** | **4.9x faster** |
| 1,000 | 5,000 | 39.67 μs | 34.69 μs | **17.52 μs** | **2.0x faster** |
| 5,000 | 1,000 | 37.64 μs | 34.49 μs | **14.18 μs** | **2.4x faster** |
| 5,000 | 5,000 | 469.82 μs | 193.18 μs | **37.43 μs** | **5.2x faster** |
| 5,000 | 10,000 | 619.32 μs | 242.05 μs | **85.73 μs** | **2.8x faster** |
| 10,000 | 5,000 | 627.93 μs | 280.03 μs | **61.78 μs** | **4.5x faster** |
| 10,000 | 10,000 | 1,115.00 μs | 434.57 μs | **103.38 μs** | **4.2x faster** |

---

### 3. Batch Vertex & Undirected Edge Insertion

| Operation | Scale | `QuikGraph` | `Naive` | `Graphs.NET` | Speedup vs QuikGraph |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Add Undirected Edges** | 1,000 V / 1,000 E | 82.18 μs | 23.05 μs | **5.44 μs** | **15.1x faster** |
| **Add Undirected Edges** | 1,000 V / 5,000 E | 991.81 μs | 121.51 μs | **37.62 μs** | **26.4x faster** |
| **Add Undirected Edges** | 1,000 V / 10,000 E | 2,227.00 μs | 232.31 μs | **85.88 μs** | **25.9x faster** |
| **Add Undirected Edges** | 5,000 V / 10,000 E | 2,718.00 μs | 544.27 μs | **94.91 μs** | **28.6x faster** |
| **Add Vertices (Batch)** | 100,000 V | 6,364.00 μs | 6,534.00 μs | **469.94 μs** | **13.5x faster** |
| **Add Vertices (Batch)** | 500,000 V | 29,100.00 μs | 34,691.00 μs | **2,561.00 μs** | **11.4x faster** |
| **Add Vertices (Batch)** | 1,000,000 V | 74,807.00 μs | 69,896.00 μs | **5,516.00 μs** | **13.6x faster** |

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

This project is licensed under the [MIT License](LICENSE.txt).
