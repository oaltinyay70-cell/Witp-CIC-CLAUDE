using System;
using System.Collections.Generic;
using System.Linq;

namespace ROCHEFORT6000.Core.Analytics
{
    public enum ConceptType
    {
        Base,
        Vessel,
        Unit,
        AirGroup,
        StrategicRegion,
        InvasionTarget,
        CarrierThreat,
        LogisticsCorridor
    }

    public class ConceptRelation
    {
        public string TargetId { get; set; } = string.Empty;
        public string RelationType { get; set; } = string.Empty; // "LOCATED_AT", "THREATENS", "SUPPLIES", "INTERCEPTS"
        public float Weight { get; set; } = 1.0f;
    }

    public class ConceptNode
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ConceptType Type { get; set; }
        public int HexX { get; set; }
        public int HexY { get; set; }
        public string Details { get; set; } = string.Empty;
        public string SearchDocument { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public List<ConceptRelation> Relations { get; set; } = new();
    }

    public class SemanticConceptGraph
    {
        private readonly List<ConceptNode> _nodes = new();
        private readonly Dictionary<string, int> _idIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _vocabulary = new();
        private readonly Dictionary<string, int> _termIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> _idfWeights = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<ConceptNode> Nodes => _nodes;
        public int Count => _nodes.Count;

        public void AddNode(ConceptNode node)
        {
            if (string.IsNullOrWhiteSpace(node.Id)) return;
            if (_idIndex.TryGetValue(node.Id, out int existingIdx))
            {
                _nodes[existingIdx] = node;
                return;
            }

            _idIndex[node.Id] = _nodes.Count;
            _nodes.Add(node);
        }

        public void AddRelation(string sourceId, string targetId, string relationType, float weight = 1.0f)
        {
            if (_idIndex.TryGetValue(sourceId, out int srcIdx))
            {
                var srcNode = _nodes[srcIdx];
                if (!srcNode.Relations.Any(r => r.TargetId.Equals(targetId, StringComparison.OrdinalIgnoreCase) && r.RelationType == relationType))
                {
                    srcNode.Relations.Add(new ConceptRelation
                    {
                        TargetId = targetId,
                        RelationType = relationType,
                        Weight = weight
                    });
                }
            }
        }

        public void BuildEmbeddings(int maxVocab = 300)
        {
            if (_nodes.Count == 0) return;

            // 1. Gather document frequencies across all concept search documents
            var termDocCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var tokenizedDocs = new List<string[]>(_nodes.Count);
            foreach (var node in _nodes)
            {
                string text = $"{node.Name} {node.Details} {node.SearchDocument}".ToLowerInvariant();
                var tokens = Tokenize(text).Distinct().ToArray();
                tokenizedDocs.Add(tokens);

                foreach (var token in tokens)
                {
                    termDocCount[token] = termDocCount.GetValueOrDefault(token, 0) + 1;
                }
            }

            // 2. Select top domain vocabulary by document frequency (ignoring common stopwords)
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "and", "or", "in", "on", "at", "to", "for", "with", "by", "of", "a", "an", "is", "was",
                "are", "were", "near", "sector", "hex", "status", "ready", "force", "turn"
            };

            var topTerms = termDocCount
                .Where(kvp => kvp.Key.Length >= 3 && !stopWords.Contains(kvp.Key) && kvp.Value >= 1)
                .OrderByDescending(kvp => kvp.Value)
                .Take(maxVocab)
                .Select(kvp => kvp.Key)
                .ToList();

            _vocabulary.Clear();
            _termIndex.Clear();
            _idfWeights.Clear();

            int docCount = _nodes.Count;
            for (int i = 0; i < topTerms.Count; i++)
            {
                string term = topTerms[i];
                _vocabulary.Add(term);
                _termIndex[term] = i;
                int df = termDocCount[term];
                // Standard smoothed IDF
                _idfWeights[term] = (float)Math.Log((1.0 + docCount) / (1.0 + df)) + 1.0f;
            }

            int vocabDim = _vocabulary.Count;
            if (vocabDim == 0) return;

            // 3. Compute TF-IDF vector for each ConceptNode
            for (int n = 0; n < _nodes.Count; n++)
            {
                var node = _nodes[n];
                string[] tokens = Tokenize($"{node.Name} {node.Details} {node.SearchDocument}".ToLowerInvariant());
                node.Embedding = ComputeTfIdfVector(tokens, vocabDim);
            }
        }

        public List<(ConceptNode Node, float Score)> Understand(string query, int maxResults = 12)
        {
            if (string.IsNullOrWhiteSpace(query) || _nodes.Count == 0 || _vocabulary.Count == 0)
            {
                return new List<(ConceptNode, float)>();
            }

            string[] queryTokens = Tokenize(query.ToLowerInvariant());
            float[] queryVector = ComputeTfIdfVector(queryTokens, _vocabulary.Count);

            var ranked = new List<(ConceptNode Node, float Score)>();

            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                if (node.Embedding.Length == 0) continue;

                float sim = CosineSimilarity(queryVector, node.Embedding);

                // Add bonus for exact token substring match in Name
                foreach (var qt in queryTokens)
                {
                    if (qt.Length >= 3 && node.Name.Contains(qt, StringComparison.OrdinalIgnoreCase))
                    {
                        sim += 0.35f;
                    }
                }

                if (sim > 0.05f)
                {
                    ranked.Add((node, sim));
                }
            }

            return ranked.OrderByDescending(r => r.Score).Take(maxResults).ToList();
        }

        private float[] ComputeTfIdfVector(string[] tokens, int dim)
        {
            var vec = new float[dim];
            if (tokens.Length == 0) return vec;

            var tf = new Dictionary<int, float>();
            foreach (var token in tokens)
            {
                if (_termIndex.TryGetValue(token, out int idx))
                {
                    tf[idx] = tf.GetValueOrDefault(idx, 0f) + 1f;
                }
            }

            float normSq = 0f;
            foreach (var kvp in tf)
            {
                int idx = kvp.Key;
                string term = _vocabulary[idx];
                float weight = kvp.Value * _idfWeights[term];
                vec[idx] = weight;
                normSq += weight * weight;
            }

            if (normSq > 0f)
            {
                float invNorm = 1.0f / (float)Math.Sqrt(normSq);
                for (int i = 0; i < dim; i++)
                {
                    vec[i] *= invNorm;
                }
            }

            return vec;
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length == 0 || b.Length == 0 || a.Length != b.Length) return 0f;

            float dot = 0f;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
            }
            return dot;
        }

        private static string[] Tokenize(string text)
        {
            return text.Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '(', ')', '[', ']', '-', '/', '\\', '_', '"', '\'' },
                              StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }
}
