using FluentAssertions;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace Vostok.ClusterConfig.Client.Abstractions.Tests
{
    [TestFixture]
    internal class ClusterConfigPath_Tests
    {
        [Test]
        public void Should_accept_null_string_representations_in_ctor()
        {
            new ClusterConfigPath(null);
        }

        [Test]
        public void Should_trim_provided_input()
        {
            new ClusterConfigPath(" foo/bar ").ToString().Should().Be("foo/bar");
        }

        [Test]
        public void ToString_should_return_an_empty_string_for_null_path()
        {
            default(ClusterConfigPath).ToString().Should().BeEmpty();
        }

        [Test]
        public void ToString_should_return_an_empty_string_for_empty_path()
        {
            new ClusterConfigPath(string.Empty).ToString().Should().BeEmpty();
        }

        [Test]
        public void ToString_should_return_an_empty_string_for_whitespace_path()
        {
            new ClusterConfigPath(" \t ").ToString().Should().BeEmpty();
        }

        [Test]
        public void Segments_property_should_correctly_split_path_into_segments()
        {
            new ClusterConfigPath("foo/bar/baz").Segments.Should().Equal("foo", "bar", "baz");
        }

        [Test]
        public void Segments_property_should_omit_empty_segments()
        {
            new ClusterConfigPath("/foo//bar//baz/").Segments.Should().Equal("foo", "bar", "baz");
        }

        [Test]
        public void Segments_property_should_return_an_empty_sequence_for_empty_paths()
        {
            default(ClusterConfigPath).Segments.Should().BeEmpty();

            new ClusterConfigPath("/").Segments.Should().BeEmpty();

            new ClusterConfigPath("///").Segments.Should().BeEmpty();
        }

        [Test]
        public void Equals_should_be_case_insensitive()
        {
            var path1 = new ClusterConfigPath("foo");
            var path2 = new ClusterConfigPath("FOO");
            var path3 = new ClusterConfigPath("bar");

            path1.Should().Be(path2);
            path1.Should().NotBe(path3);
        }

        [Test]
        public void GetHashCode_should_be_case_insensitive()
        {
            var path1 = new ClusterConfigPath("foo");
            var path2 = new ClusterConfigPath("FOO");
            var path3 = new ClusterConfigPath("bar");

            path1.GetHashCode().Should().Be(path2.GetHashCode());
            path1.GetHashCode().Should().NotBe(path3.GetHashCode());
        }

        [TestCase(null, null, true)]
        [TestCase(null, "", true)]
        [TestCase(null, "foo", true)]
        [TestCase(null, "foo/bar", true)]
        [TestCase("", "foo/bar", true)]
        [TestCase("///", "foo/bar", true)]

        [TestCase("foo", null, false)]
        [TestCase("foo", "foo", true)]
        [TestCase("foo", "foofoo", false)]
        [TestCase("foo", "foo/bar", true)]
        [TestCase("foo", "foo/bar/baz/", true)]

        [TestCase("foo/bar", "", false)]
        [TestCase("foo/bar", "foo", false)]
        [TestCase("foo/bar", "Foo/BAR", true)]
        [TestCase("foo/bar", "Foo/BAR/baz/", true)]

        [TestCase("foo/bar/baz", "foo/baz", false)]
        [TestCase("foo/bar/baz", "foo/baz/bar", false)]
        [TestCase("foo/bar/baz", "foo/bar/whatever", false)]
        public void IsPrefix_should_return_correct_result_based_on_given_input(string path1, string path2, bool expectedResult)
        {
            new ClusterConfigPath(path1).IsPrefixOf(new ClusterConfigPath(path2)).Should().Be(expectedResult);
        }
    }
}