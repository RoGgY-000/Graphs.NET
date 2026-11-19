using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Graphs.NET.Core;

namespace Tests.Data
{
	public class GraphBuilderWithWeightAndDataTestData
	{
		public static IEnumerable<object[]> GetGraphBuildersAndExpected ()
		{
			// 1) empty graph
			{
				GraphBuilder<string, object> builder = new();

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 0,
						ArcCount = 0,
						VertexCapacity = 0,
						VertexCount = 0,
						VerticesAndArcs = [],
						Weights = [],
						VertexData = []
					}
				};
			}

			// 2) 1 vertex, 0 arcs
			{
				GraphBuilder<string, object> builder = new();
				object data = new();
				builder.AddVertex(0, data);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 0,
						ArcCount = 0,
						VertexCapacity = 1,
						VertexCount = 1,
						VerticesAndArcs = new()
						{
							{ 0, [] }
						},
						Weights = [],
						VertexData = new()
						{
							{ 0, data }
						}
					}
				};
			}

			// 3) 1 vertex, 1 reflexive arc
			{
				GraphBuilder<string, object> builder = new();
				object data = new();
				builder.AddVertex(0, data);
				builder.AddArc(0, 0, "0->0");

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 1,
						ArcCount = 1,
						VertexCapacity = 1,
						VertexCount = 1,
						VerticesAndArcs = new()
						{
							{ 0, [0] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 0, ["0->0"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data }
						}
					}
				};
			}

			// 4) 1 vertex, 2 reflexive arcs
			{
				GraphBuilder<string, object> builder = new();
				builder.AddArcAndVertices(0, 0, "0->0");
				builder.AddArcAndVertices(0, 0, "0->0");
				object data = new();
				builder.SetVertexData(0, data);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 2,
						ArcCount = 2,
						VertexCapacity = 1,
						VertexCount = 1,
						VerticesAndArcs = new()
						{
							{ 0, [0, 0] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 0, ["0->0", "0->0"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data }
						}
					}
				};
			}

			// 5) 2 vertexes, 0 arcs
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				object data1 = new();
				builder.AddVertices([0, 1]);
				builder.SetVertexData(0, data0);
				builder.SetVertexData(1, data1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 0,
						ArcCount = 0,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [] },
							{ 1, [] }
						},
						Weights = [],
						VertexData = new()
						{
							{ 0, data0 },
							{ 1, data1 }
						}
					}
				};
			}

			// 6) 2 vertexes, 1 reflexive arc
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				builder.AddArcAndVertices(1, 1, "1->1");
				builder.AddVertex(0, data0);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 1,
						ArcCount = 1,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [] },
							{ 1, [1] }
						},
						Weights = new()
						{
							{ 1, new()
								{
									{ 1, ["1->1"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data0 }
						}
					}
				};
			}

			// 7) 2 vertexes, 2 reflexive arcs
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				object data1 = new();
				builder.AddArcAndVertices(0, 0, "0->0");
				builder.AddArcAndVertices(1, 1, "1->1");
				builder.SetVertexData(0, data0);
				builder.SetVertexData(1, data1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 2,
						ArcCount = 2,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [0] },
							{ 1, [1] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 0, ["0->0"] }
								}
							},
							{ 1, new()
								{
									{ 1, ["1->1"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data0 },
							{ 1, data1 }
						}
					}
				};
			}

			// 8) 2 vertexes, 3 reflexive arcs
			{
				GraphBuilder<string, object> builder = new();
				object data1 = new();
				builder.AddArcAndVertices(0, 0, "0->0");
				builder.AddArcAndVertices(0, 0, "0->0");
				builder.AddArcAndVertices(1, 1, "1->1");
				builder.SetVertexData(1, data1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 4,
						ArcCount = 3,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [0, 0] },
							{ 1, [1] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 0, ["0->0", "0->0"] }
								}
							},
							{ 1, new()
								{
									{ 1, ["1->1"] }
								}
							}
						},
						VertexData = new()
						{
							{ 1, data1 }
						}
					}
				};
			}

			// 9) 2 vertexes, 1 arc
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				builder.AddArcAndVertices(0, 1, "0->1");
				builder.SetVertexData(0, data0);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 1,
						ArcCount = 1,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [1] },
							{ 1, [] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 1, ["0->1"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data0 }
						}
					}
				};
			}

			// 10) 2 vertexes, 2 arcs
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				object data1 = new();
				builder.AddArcAndVertices(0, 1, "0->1");
				builder.AddArcAndVertices(1, 0, "1->0");
				builder.SetVertexData(0, data0);
				builder.SetVertexData(1, data1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 2,
						ArcCount = 2,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [1] },
							{ 1, [0] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 1, ["0->1"] }
								}
							},
							{ 1, new()
								{
									{ 0, ["1->0"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data0 },
							{ 1, data1 }
						}
					}
				};
			}

			// 11) 2 vertexes, 3 arcs
			{
				GraphBuilder<string, object> builder = new();
				builder.AddArcAndVertices(0, 1, "0->1");
				builder.AddArcAndVertices(1, 0, "1->0");
				builder.AddArcAndVertices(0, 1, "0->1");

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 4,
						ArcCount = 3,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [1, 1] },
							{ 1, [0] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 1, ["0->1", "0->1"] }
								}
							},
							{ 1, new()
								{
									{ 0, ["1->0"] }
								}
							}
						},
						VertexData = []
					}
				};
			}

			// 12) 2 vertexes, 4 arcs
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				object data1 = new();
				builder.AddArcAndVertices(0, 1, "0->1");
				builder.AddArcAndVertices(1, 0, "1->0");
				builder.AddArcAndVertices(0, 1, "0->1");
				builder.AddArcAndVertices(1, 0, "1->0");
				builder.SetVertexData(0, data0);
				builder.SetVertexData(1, data1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 4,
						ArcCount = 4,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [1, 1] },
							{ 1, [0, 0] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 1, ["0->1", "0->1"] }
								}
							},
							{ 1, new()
								{
									{ 0, ["1->0", "1->0"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data0 },
							{ 1, data1 }
						}
					}
				};
			}

			// 13) 2 vertexes, 2 arcs, 1 reflexive arc
			{
				GraphBuilder<string, object> builder = new();
				object data1 = new();
				builder.AddArcAndVertices(0, 1, "0->1");
				builder.AddArcAndVertices(1, 1, "1->1");
				builder.SetVertexData(1, data1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 2,
						ArcCount = 2,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [1] },
							{ 1, [1] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 1, ["0->1"] }
								}
							},
							{ 1, new()
								{
									{ 1, ["1->1"] }
								}
							}
						},
						VertexData = new()
						{
							{ 1, data1 }
						}
					}
				};
			}

			// 14) 2 vertexes, 4 arcs, 2 reflexive arcs
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				object data1 = new();
				builder.AddArcAndVertices(0, 0, "0->0");
				builder.AddArcAndVertices(0, 1, "0->1");
				builder.AddArcAndVertices(1, 0, "1->0");
				builder.AddArcAndVertices(1, 1, "1->1");
				builder.SetVertexData(0, data0);
				builder.SetVertexData(1, data1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 4,
						ArcCount = 4,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [0, 1] },
							{ 1, [0, 1] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 0, ["0->0"] },
									{ 1, ["0->1"] }
								}
							},
							{ 1, new()
								{
									{ 0, ["1->0"] },
									{ 1, ["1->1"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data0 },
							{ 1, data1 }
						}
					}
				};
			}

			// 15) 2 vertexes, 8 arcs, 4 reflexive arcs
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				object data1 = new();
				builder.AddArcAndVertices(0, 0, "0->0"); builder.AddArcAndVertices(0, 0, "0->0");
				builder.AddArcAndVertices(0, 1, "0->1"); builder.AddArcAndVertices(0, 1, "0->1");
				builder.AddArcAndVertices(1, 0, "1->0"); builder.AddArcAndVertices(1, 0, "1->0");
				builder.AddArcAndVertices(1, 1, "1->1"); builder.AddArcAndVertices(1, 1, "1->1");
				builder.SetVertexData(0, data0);
				builder.SetVertexData(1, data1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 8,
						ArcCount = 8,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [0, 0, 1, 1] },
							{ 1, [0, 0, 1, 1] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 0, ["0->0", "0->0"] },
									{ 1, ["0->1", "0->1"] }
								}
							},
							{ 1, new()
								{
									{ 0, ["1->0", "1->0"] },
									{ 1, ["1->1", "1->1"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data0 },
							{ 1, data1 }
						}
					}
				};
			}

			// 16) 5 vertexes, 9 arcs, 2 reflexive arcs
			{
				GraphBuilder<string, object> builder = new();
				object data0 = new();
				object data1 = new();
				object data3 = new();
				object data4 = new();

				// cycle: 0 -> 2 -> 4 -> 0
				builder.AddArcAndVertices(0, 2, "0->2");
				builder.AddArcAndVertices(2, 4, "2->4");
				builder.AddArcAndVertices(4, 0, "4->0");

				// ordinal arcs
				builder.AddArcAndVertices(0, 1, "0->1");
				builder.AddArcAndVertices(2, 1, "2->1");
				builder.AddArcAndVertices(2, 3, "2->3");
				builder.AddArcAndVertices(4, 3, "4->3");

				// reflexive arcs
				builder.AddArcAndVertices(2, 2, "2->2");
				builder.AddArcAndVertices(3, 3, "3->3");

				builder.SetVertexData(0, data0);
				builder.SetVertexData(1, data1);
				builder.SetVertexData(3, data3);
				builder.SetVertexData(4, data4);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder<string, object> {
						ArcCapacity = 16,
						ArcCount = 9,
						VertexCapacity = 8,
						VertexCount = 5,
						VerticesAndArcs = new()
						{
							{ 0, [1, 2] },
							{ 1, [] },
							{ 2, [1, 2, 3, 4] },
							{ 3, [3] },
							{ 4, [0, 3] }
						},
						Weights = new()
						{
							{ 0, new()
								{
									{ 1, ["0->1"] },
									{ 2, ["0->2"] }
								}
							},
							{ 2, new()
								{
									{ 1, ["2->1"] },
									{ 2, ["2->2"] },
									{ 3, ["2->3"] },
									{ 4, ["2->4"] }
								}
							},
							{ 3, new()
								{
									{ 3, ["3->3"] }
								}
							},
							{ 4, new()
								{
									{ 0, ["4->0"] },
									{ 3, ["4->3"] }
								}
							}
						},
						VertexData = new()
						{
							{ 0, data0 },
							{ 1, data1 },
							{ 3, data3 },
							{ 4, data4 }
						}
					}
				};
			}
		}

		public static IEnumerable<object[]> GetRandomGraphBuildersAndExpected ()
		{
			// 17)
			yield return GenerateRandomGraphBuilderAndExpected(1, 1);
			// 18)
			yield return GenerateRandomGraphBuilderAndExpected(1, 10);
			// 19)
			yield return GenerateRandomGraphBuilderAndExpected(1, 100);
			// 20)
			yield return GenerateRandomGraphBuilderAndExpected(1, 1_000);
			// 21)
			yield return GenerateRandomGraphBuilderAndExpected(10, 1);
			// 22)
			yield return GenerateRandomGraphBuilderAndExpected(10, 10);
			// 23)
			yield return GenerateRandomGraphBuilderAndExpected(10, 100);
			// 24)
			yield return GenerateRandomGraphBuilderAndExpected(10, 1_000);
			// 25)
			yield return GenerateRandomGraphBuilderAndExpected(100, 1);
			// 26)
			yield return GenerateRandomGraphBuilderAndExpected(100, 10);
			// 27)
			yield return GenerateRandomGraphBuilderAndExpected(100, 100);
			// 28)
			yield return GenerateRandomGraphBuilderAndExpected(100, 1_000);
			// 29)
			yield return GenerateRandomGraphBuilderAndExpected(1_000, 1);
			// 30)
			yield return GenerateRandomGraphBuilderAndExpected(1_000, 10);
			// 31)
			yield return GenerateRandomGraphBuilderAndExpected(1_000, 100);
			// 32)
			yield return GenerateRandomGraphBuilderAndExpected(1_000, 1_000);
		}

		private static object[] GenerateRandomGraphBuilderAndExpected (int arcCount, int vertexCount)
		{
			GraphBuilder<string, object> builder = new();
			Dictionary<int, List<int>> naive = new(vertexCount);
			Dictionary<int, Dictionary<int, List<string>>> weightBuilder = [];
			Random r = new(42);

			for ( int i = 0; i < arcCount; i++ )
			{
				int from = r.Next(0, vertexCount), to = r.Next(0, vertexCount);

				builder.AddArcAndVertices(from, to, $"{from}->{to}");

				if ( !naive.TryGetValue(from, out List<int>? value) )
				{
					value = [];
					naive.Add(from, value);
				}

				if ( !naive.ContainsKey(to) )
				{
					naive.Add(to, []);
				}

				value.Add(to);

				if ( !weightBuilder.ContainsKey(from) )
				{
					weightBuilder.Add(from, []);
				}
				if ( !weightBuilder[from].ContainsKey(to) )
				{
					weightBuilder[from].Add(to, []);
				}

				weightBuilder[from][to].Add($"{from}->{to}");
			}

			Dictionary<int, int[]> VertiсesAndArcs = new();
			foreach ( int vertex in naive.Keys )
			{
				int[] arr = naive[vertex].ToArray();
				arr.Sort();
				VertiсesAndArcs.Add(vertex, arr);
			}

			Dictionary<int, Dictionary<int, string[]>> weights = [];
			foreach ( int vertex in weightBuilder.Keys )
			{
				Dictionary<int, List<string>> neighbors = weightBuilder[vertex];
				Dictionary<int, string[]> neighborsToArray = [];
				foreach ( int neighbor in neighbors.Keys )
				{
					neighborsToArray.Add(neighbor, neighbors[neighbor].ToArray());
				}
				weights.Add(vertex, neighborsToArray);
			}

			return
			[
				builder,
				new ExpectedGraphBuilder<string, object>
				{
					ArcCapacity = (int) BitOperations.RoundUpToPowerOf2( (uint) arcCount),
					ArcCount = arcCount,
					VertexCapacity = (int) BitOperations.RoundUpToPowerOf2( (uint) naive.Keys.Max() + 1),
					VertexCount = naive.Count,
					VerticesAndArcs = VertiсesAndArcs,
					Weights = weights
				}
			];
		}
	}

	public class ExpectedGraphBuilder<TWeight, TData> : ExpectedGraphBuilder<TWeight>
	{
		public Dictionary<int, TData> VertexData { get; set; }
	}
}
