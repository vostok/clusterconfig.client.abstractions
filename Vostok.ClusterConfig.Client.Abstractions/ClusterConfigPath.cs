using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Returns <c>true</c> if <see cref="Segments"/> sequence of current path is a prefix of <paramref name="otherPath"/> <see cref="Segments"/>, or <c>false</c> otherwise.
        /// </summary>
        public bool IsPrefixOf(ClusterConfigPath otherPath)
        {
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
