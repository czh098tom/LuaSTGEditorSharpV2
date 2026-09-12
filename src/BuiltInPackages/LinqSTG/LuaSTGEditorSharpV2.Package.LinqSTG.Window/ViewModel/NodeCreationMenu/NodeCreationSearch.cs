using System;
using System.Collections.Generic;
using System.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu
{
    /// <summary>
    /// Case-insensitive subsequence fuzzy matcher used by the node creation menu search.
    /// All pattern characters must appear in the candidate in order; word starts and
    /// contiguous runs score higher so better matches can be ranked first.
    /// </summary>
    public static class NodeCreationSearch
    {
        /// <summary>
        /// Scores the candidate against the query. Returns null when the query is not a
        /// subsequence of the candidate; otherwise a higher score means a better match.
        /// </summary>
        public static int? Score(string query, string candidate)
        {
            if (string.IsNullOrEmpty(query)) return 0;
            if (string.IsNullOrEmpty(candidate)) return null;

            // An exact title match always outranks partial ones, so searching
            // "float" puts "Float" ahead of suffixed titles like "External Variable (Float)".
            var exactBonus = string.Equals(query, candidate, StringComparison.OrdinalIgnoreCase) ? 1000 : 0;

            int score = 0;
            int queryIndex = 0;
            int previousMatchIndex = int.MinValue;

            for (int candidateIndex = 0; candidateIndex < candidate.Length && queryIndex < query.Length; candidateIndex++)
            {
                if (char.ToLowerInvariant(candidate[candidateIndex]) != char.ToLowerInvariant(query[queryIndex]))
                {
                    continue;
                }

                if (candidateIndex == 0 || IsWordBoundary(candidate[candidateIndex - 1]))
                {
                    score += 4;
                }
                else if (candidateIndex == previousMatchIndex + 1)
                {
                    score += 3;
                }
                else
                {
                    score += 1;
                }

                previousMatchIndex = candidateIndex;
                queryIndex++;
            }

            return queryIndex < query.Length ? null : score + exactBonus;
        }

        /// <summary>
        /// Returns the entries matching the query against both their current-culture title
        /// and their English title, best match first. Entries matching neither are excluded.
        /// </summary>
        public static IEnumerable<NodeCreationEntry> Search(string query, IEnumerable<NodeCreationEntry> entries)
        {
            var trimmed = query.Trim();
            if (trimmed.Length == 0)
            {
                return Enumerable.Empty<NodeCreationEntry>();
            }

            return entries
                .Select(entry => (Entry: entry,
                    Score: Math.Max(
                        Score(trimmed, entry.Title) ?? int.MinValue,
                        Score(trimmed, entry.EnglishTitle) ?? int.MinValue)))
                .Where(pair => pair.Score > int.MinValue)
                .OrderByDescending(pair => pair.Score)
                .ThenBy(pair => pair.Entry.EnglishTitle, StringComparer.OrdinalIgnoreCase)
                .Select(pair => pair.Entry);
        }

        private static bool IsWordBoundary(char previous)
        {
            return char.IsWhiteSpace(previous) || previous is '_' or '-' or '/' or '(' or '.' or '+';
        }
    }
}
