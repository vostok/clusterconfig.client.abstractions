using System;
using System.Collections.Generic;
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
