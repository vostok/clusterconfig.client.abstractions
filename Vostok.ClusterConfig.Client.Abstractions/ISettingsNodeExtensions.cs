using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Client.Abstractions
{
    [PublicAPI]
    public static class ISettingsNodeExtensions
    {
        /// <summary>
        /// <para>Flattens given settings tree to a key-value dictionary. Every item in this dictionary corresponds to either a <see cref="ValueNode"/> or an <see cref="ArrayNode"/>.</para>
        /// <para>Value list for a <see cref="ValueNode"/> consists just of its single <see cref="ISettingsNode.Value"/>.</para>
        /// <para>Value list for an <see cref="ArrayNode"/> consists of <see cref="ISettingsNode.Value"/>s of its child nodes.</para>
        /// <para>Every key is assembled from leaf node name and names of all intermediate <see cref="ObjectNode"/>s by joining them with <see cref="ClusterConfigPath.Separator"/>.</para>
        /// <para>The only exception is the name of given <paramref name="node"/> itself: it's not included in keys.</para>
        /// </summary>
        [NotNull]
        public static Dictionary<ClusterConfigPath, List<string>> Flatten([CanBeNull] this ISettingsNode node)
        {
            var result = new Dictionary<ClusterConfigPath, List<string>>();

            VisitNodeInternal(node, new List<string>(), result);

            return result;
        }

        private static void VisitNode(ISettingsNode node, List<string> path, Dictionary<ClusterConfigPath, List<string>> result)
        {
            if (node == null)
                return;

            path.Add(node.Name);

            VisitNodeInternal(node, path, result);

            path.RemoveAt(path.Count - 1);
        }

        private static void VisitNodeInternal(ISettingsNode node, List<string> path, Dictionary<ClusterConfigPath, List<string>> result)
        {
            switch (node)
            {
                case ObjectNode objectNode:

                    foreach (var child in objectNode.Children)
                        VisitNode(child, path, result);

                    break;

                case ValueNode valueNode:
                    result[AssemblePath(path)] = new List<string> { valueNode.Value };
                    break;

                case ArrayNode arrayNode:
                    result[AssemblePath(path)] = new List<string>(arrayNode.Children.Select(child => child.Value));
                    break;
            }
        }

        private static ClusterConfigPath AssemblePath(List<string> segments)
            => new ClusterConfigPath(string.Join(ClusterConfigPath.SeparatorString, segments.Where(s => !string.IsNullOrEmpty(s))));
    }
}