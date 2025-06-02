using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.ClusterConfig.Client.Abstractions
{
    /// <summary>
    /// <para>Represents a tree path for ClusterConfig settings. May point to a subtree (prefix) or a single leaf value (full path).</para>
    /// <para>Paths in ClusterConfig are case-insensitive.</para>
    /// </summary>
    [PublicAPI]
    public struct ClusterConfigPath : IEquatable<ClusterConfigPath>
    {
        public const char Separator = '/';

        public const string SeparatorString = "/";

        public static readonly ClusterConfigPath Empty;

        [CanBeNull]
        private readonly string path;

        public ClusterConfigPath([CanBeNull] string path)
        {
            this.path = path?.Trim();
        }

        /// <summary>
        /// Represents this path as a sequence of segments separated by <see cref="Separator"/>. May return an empty sequence.
        /// </summary>
        [NotNull]
        public IEnumerable<string> Segments
        {
            get
            {
                if (string.IsNullOrEmpty(path))
                    yield break;

                var segmentBeginning = 0;

                for (var i = 0; i < path.Length; i++)
                {
                    var current = path[i];
                    if (current == Separator)
                    {
                        if (i > segmentBeginning)
                            yield return path.Substring(segmentBeginning, i - segmentBeginning);

                        segmentBeginning = i + 1;
                    }
                }

                if (segmentBeginning < path.Length)
                    yield return path.Substring(segmentBeginning, path.Length - segmentBeginning);
            }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Represents this path as a sequence of segments separated by <see cref="Separator"/>. May return an empty sequence.
        /// </summary>
        [NotNull]
        public IEnumerable<ReadOnlyMemory<char>> SegmentsAsMemory
        {
            get
            {
                return new SegmentEnumerable(path);
            }
        }
        
        private class SegmentEnumerable : IEnumerable<ReadOnlyMemory<char>>
        {
            private readonly string path;

            public SegmentEnumerable(string path)
            {
                this.path = path;
            }

            public IEnumerator<ReadOnlyMemory<char>> GetEnumerator()
            {
                return new SegmentEnumerator(path);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private struct SegmentEnumerator : IEnumerator<ReadOnlyMemory<char>>
        {
            private readonly string path;
            private int index = 0;
            private int segmentBeginning = 0;
            private int from = 0;

            public SegmentEnumerator(string path)
            {
                this.path = path;
            }

            public bool MoveNext()
            { 
                if (string.IsNullOrEmpty(path) || index == path.Length)
                    return false;
                
                for (; index < path.Length; index++)
                {
                    var current = path[index];
                    if (current == Separator)
                    {
                        if (index > segmentBeginning)
                        {
                            from = segmentBeginning;
                            segmentBeginning = index + 1;

                            return true;
                        }

                        segmentBeginning = index + 1;
                    }
                }

                if (segmentBeginning < path.Length)
                {
                    from = segmentBeginning;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                index = 0;
                segmentBeginning = 0;
            }

            public ReadOnlyMemory<char> Current { get => path.AsMemory().Slice(from, index - from); } 
            object IEnumerator.Current =>
                Current;

            public void Dispose()
            {
            }
        }
#endif

        /// <summary>
        /// Returns <c>true</c> if <see cref="Segments"/> sequence of current path is a prefix of <paramref name="otherPath"/> <see cref="Segments"/>, or <c>false</c> otherwise.
        /// </summary>
        public bool IsPrefixOf(ClusterConfigPath otherPath)
        {
#if NET6_0_OR_GREATER
            using var segments = new SegmentEnumerator(path);
            using var otherSegments = new SegmentEnumerator(otherPath.path);
            
            while (true)
            {
                if (!segments.MoveNext())
                    return true;

                if (!otherSegments.MoveNext())
                    return false;

                if (!segments.Current.Span.Equals(otherSegments.Current.Span, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
#else
            using (var segments = Segments.GetEnumerator())
            using (var otherSegments = otherPath.Segments.GetEnumerator())
            {
                while (true)
                {
                    if (!segments.MoveNext())
                        return true;

                    if (!otherSegments.MoveNext())
                        return false;

                    if (!StringComparer.OrdinalIgnoreCase.Equals(segments.Current, otherSegments.Current))
                        return false;
                }
            }
#endif
        }

        public bool TryScopeTo(ClusterConfigPath prefix, out ClusterConfigPath newPath)
        {
#if NET6_0_OR_GREATER
            using var segments = new SegmentEnumerator(path);
            using var prefixToCutSegments = new SegmentEnumerator(prefix.path);
            
            while (true)
            {
                if (!prefixToCutSegments.MoveNext())
                {
                    newPath = BuildNormalizedPathFromSegments(segments);
                    return true;
                }

                if (!segments.MoveNext())
                {
                    newPath = default;
                    return false;
                }

                if (!segments.Current.Span.Equals(prefixToCutSegments.Current.Span, StringComparison.OrdinalIgnoreCase))
                {
                    newPath = default;
                    return false;
                }
            }
#else
            using (var segments = Segments.GetEnumerator())
            using (var prefixToCutSegments = prefix.Segments.GetEnumerator())
            {
                while (true)
                {
                    if (!prefixToCutSegments.MoveNext())
                    {
                        newPath = BuildNormalizedPathFromSegments(segments);
                        return true;
                    }

                    if (!segments.MoveNext())
                    {
                        newPath = default;
                        return false;
                    }

                    if (!StringComparer.OrdinalIgnoreCase.Equals(segments.Current, prefixToCutSegments.Current))
                    {
                        newPath = default;
                        return false;
                    }
                }
            }
#endif
        }

        public bool Equivalent(ClusterConfigPath other)
        {
#if NET6_0_OR_GREATER
            using var segments = new SegmentEnumerator(path);
            using var otherSegments = new SegmentEnumerator(other.path);
            
            while (true)
            {
                var hasNext = segments.MoveNext();
                var otherHasNext = otherSegments.MoveNext();

                if (hasNext != otherHasNext)
                    return false;

                if (!hasNext)
                    return true;
                
                if (!segments.Current.Span.Equals(otherSegments.Current.Span, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
#else
            using (var segments = Segments.GetEnumerator())
            using (var otherSegments = other.Segments.GetEnumerator())
            {
                while (true)
                {
                    var hasNext = segments.MoveNext();
                    var otherHasNext = otherSegments.MoveNext();
    
                    if (hasNext != otherHasNext)
                        return false;
    
                    if (!hasNext)
                        return true;

                    if (!StringComparer.OrdinalIgnoreCase.Equals(segments.Current, otherSegments.Current))
                        return false;
                }
            }
#endif
        }

        public string GetNormalizedPath()
        {
#if NET6_0_OR_GREATER
            return BuildNormalizedPathFromSegments(new SegmentEnumerator(path)).path;
#else
            return BuildNormalizedPathFromSegments(Segments.GetEnumerator()).path;
#endif
        }

#if NET6_0_OR_GREATER
        private static ClusterConfigPath BuildNormalizedPathFromSegments(IEnumerator<ReadOnlyMemory<char>> segments)
        {
            var sb = new StringBuilder();
            
            var first = true;
            while (segments.MoveNext())
            {
                if (!first)
                    sb.Append(Separator);
                first = false;
                sb.Append(segments.Current);
            }

            return new ClusterConfigPath(sb.ToString());
        }
#endif

        private static ClusterConfigPath BuildNormalizedPathFromSegments(IEnumerator<string> segments)
        {
            var sb = new StringBuilder();
            
            var first = true;
            while (segments.MoveNext())
            {
                if (!first)
                    sb.Append(Separator);
                first = false;
                sb.Append(segments.Current);
            }

            return new ClusterConfigPath(sb.ToString());
        }

        public override string ToString() 
            => path ?? string.Empty;

        public static implicit operator ClusterConfigPath(string value)
            => new ClusterConfigPath(value);

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj) && obj is ClusterConfigPath other && Equals(other);

        public bool Equals(ClusterConfigPath other)
            => StringComparer.OrdinalIgnoreCase.Equals(path, other.path);

        public override int GetHashCode() 
            => path == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(path);
    }
}
