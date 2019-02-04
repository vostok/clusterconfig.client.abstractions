using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Client.Abstractions
{
    /// <summary>
    /// <para>Represents a client for ClusterConfig service used to obtain application settings.</para>
    /// <para>Settings in ClusterConfig comprise a tree, and the main operation this client presents is obtaining settings by prefix.</para>
    /// <para>Prefixes are tree paths: they consist of node names separated by forward slashes (e.g. <c>foo/bar/baz</c>).</para>
    /// <para>This client's methods come in three flavors:</para>
    /// <list type="bullet">
    ///     <item><description>Synchronous (<see cref="Get"/>, <see cref="GetWithVersion"/>)</description></item>
    ///     <item><description>Asynchronous (<see cref="GetAsync"/>, <see cref="GetWithVersionAsync"/>)</description></item>
    ///     <item><description>Reactive (<see cref="Observe"/>, <see cref="ObserveWithVersions"/>)</description></item>
    /// </list>
    /// </summary>
    [PublicAPI]
    public interface IClusterConfigClient
    {
        /// <summary>
        /// <para>Obtains and returns the subtree of settings located under given <paramref name="prefix"/>.</para>
        /// <para>If given <paramref name="prefix"/> is empty, returns all settings from current zone (whole tree).</para>
        /// <para>If settings tree has not yet been obtained for the first time, this method blocks until that happens.</para>
        /// <para>If settings tree has already been obtained at least once, this method immediately returns data from cache.</para>
        /// <para>If initial settings load attempt fails, this method is expected to throw an exception instead of blocking indefinitely.</para>
        /// </summary>
        /// <exception cref="ClusterConfigClientException">Initial settings load attempt has failed while there's no cache.</exception>
        [CanBeNull]
        ISettingsNode Get(ClusterConfigPath prefix);

        /// <summary>
        /// <inheritdoc cref="Get"/>
        /// <para>
        /// This method also returns a version to facilitate application-level caching.
        /// This version grows monotonically: it is guaranteed to increase on every observed change in settings.
        /// </para>
        /// </summary>
        /// <exception cref="ClusterConfigClientException">Initial settings load attempt has failed while there's no cache.</exception>
        (ISettingsNode settings, long version) GetWithVersion(ClusterConfigPath prefix);

        /// <inheritdoc cref="Get"/>
        [NotNull]
        [ItemCanBeNull]
        Task<ISettingsNode> GetAsync(ClusterConfigPath prefix);

        /// <inheritdoc cref="GetWithVersion"/>
        [NotNull]
        Task<(ISettingsNode settings, long version)> GetWithVersionAsync(ClusterConfigPath prefix);

        /// <summary>
        /// <para>Returns an observable sequence of settings belonging to the subtree located under given <paramref name="prefix"/>.</para>
        /// <para>If given <paramref name="prefix"/> is <c>null</c>, returns all settings from current zone (whole tree).</para>
        /// <para>If settings tree has not yet been obtained for the first time, returned sequence does not immediately produce any values.</para>
        /// <para>If settings tree has already been obtained at least once, returned sequence immediately produces a value taken from cache.</para>
        /// <para>If initial settings load attempt fails, returned sequence completes with <see cref="IObserver{T}.OnError"/> notification. Existing subscribers must resubscribe later in order to receive further updates.</para>
        /// <para>When a background update occurs and a new version of settings is fetched, a new value of the subtree is pushed to the sequence with <see cref="IObserver{T}.OnNext"/> notification.</para>
        /// </summary>
        [NotNull]
        IObservable<ISettingsNode> Observe(ClusterConfigPath prefix);

        /// <summary>
        /// <inheritdoc cref="Observe"/>
        /// <para>
        /// This method also provides a version alongside with settings to facilitate application-level caching.
        /// This version grows monotonically in scope of returned observable sequence: it is guaranteed to increase on every observed change in settings.
        /// </para>
        /// </summary>
        [NotNull]
        IObservable<(ISettingsNode settings, long version)> ObserveWithVersions(ClusterConfigPath prefix);
    }
}
