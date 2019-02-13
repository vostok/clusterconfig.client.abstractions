using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Client.Abstractions.Tests
{
    [TestFixture]
    internal class ISettingsNodeExtensions_Tests
    {
        [Test]
        public void Flatten_should_return_an_empty_dictionary_for_null_node()
        {
            (null as ISettingsNode).Flatten().Should().BeEmpty();
        }

        [Test]
        public void Flatten_should_return_a_single_record_for_value_nodes()
        {
            var node = new ValueNode("key", "value");

            node.Flatten().Should().HaveCount(1);
            node.Flatten()[""].Should().ContainSingle().Which.Should().Be("value");
        }

        [Test]
        public void Flatten_should_return_a_single_record_for_array_nodes()
        {
            var node = new ArrayNode("key", new ISettingsNode[]
            {
                new ValueNode("0", "value1"), 
                new ValueNode("1", "value2"), 
            });

            node.Flatten().Should().HaveCount(1);
            node.Flatten()[""].Should().Equal("value1", "value2");
        }

        [Test]
        public void Flatten_should_deal_with_object_node_hierarchies_by_merging_names()
        {
            var node = new ObjectNode(null, new ISettingsNode[]
            {
                new ObjectNode("foo", new ISettingsNode[]
                {
                    new ObjectNode("bar", new ISettingsNode[]
                    {
                        new ValueNode("k1", "v1"),
                        new ValueNode("k2", "v2"),
                        new ValueNode(string.Empty, "foo-bar-value")
                    }),
                    new ObjectNode("baz", new ISettingsNode[]
                    {
                        new ValueNode("k1", "v1"),
                        new ValueNode("k2", "v2")
                    }),
                    new ValueNode(string.Empty, "foo-value"),
                }), 
                new ObjectNode("bar", new ISettingsNode[]
                {
                    new ObjectNode("baz", new ISettingsNode[]
                    {
                        new ValueNode("k1", "v1"), 
                        new ValueNode("k2", "v2") 
                    }),
                    new ValueNode(string.Empty, "bar-value"), 
                }) 
            });

            var dictionary = node.Flatten();

            dictionary.Should().HaveCount(9);

            dictionary["foo"].Should().Equal("foo-value");

            dictionary["foo/bar"].Should().Equal("foo-bar-value");
            dictionary["foo/bar/k1"].Should().Equal("v1");
            dictionary["foo/bar/k2"].Should().Equal("v2");

            dictionary["foo/baz/k1"].Should().Equal("v1");
            dictionary["foo/baz/k2"].Should().Equal("v2");

            dictionary["bar"].Should().Equal("bar-value");
            dictionary["bar/baz/k1"].Should().Equal("v1");
            dictionary["bar/baz/k2"].Should().Equal("v2");
        }
    }
}