using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// Provides static members to access enlightenments.
    /// </summary>
    public static class Enlightenment
    {
        /// <summary>
        /// The default enlightenment provider.
        /// </summary>
        private static readonly IEnlightenmentProvider DefaultProvider = new DefaultEnlightenmentProvider();

        /// <summary>
        /// Cached instance of the platform enlightenment provider. This is equal to <see cref="DefaultProvider"/> if the platform couldn't be found.
        /// </summary>
        private static IEnlightenmentProvider _platform;

        /// <summary>
        /// Cached instance of the exception enlightenment.
        /// </summary>
        private static IExceptionEnlightenment _exception;

        /// <summary>
        /// Cached instance of the async enlightenment.
        /// </summary>
        private static IAsyncEnlightenment _async;

        /// <summary>
        /// Cached instance of the synchronization context enlightenment.
        /// </summary>
        private static ISynchronizationContextEnlightenment _synchronizationContext;

        /// <summary>
        /// Cached instance of the trace enlightenment.
        /// </summary>
        private static ITraceEnlightenment _trace;

        /// <summary>
        /// Cached instance of the concurrent collections enlightenment.
        /// </summary>
        private static IConcurrentCollectionsEnlightenment _concurrentCollections;

        /// <summary>
        /// Cached instance of the lazy enlightenment.
        /// </summary>
        private static ILazyEnlightenment _lazy;

        /// <summary>
        /// Cached instance of the thread pool enlightenment.
        /// </summary>
        private static IThreadPoolEnlightenment _threadPool;

        /// <summary>
        /// Cached instance of the thread identity enlightenment.
        /// </summary>
        private static IThreadIdentityEnlightenment _threadIdentity;

        /// <summary>
        /// Cached instance of the STA enlightenment.
        /// </summary>
        private static ISingleThreadedApartmentEnlightenment _singleThreadedApartment;

        /// <summary>
        /// Loads the <c>PlatformEnlightenmentProvider</c> if it can be found; otherwise, returns an instance of <see cref="DefaultEnlightenmentProvider"/>.
        /// </summary>
        private static IEnlightenmentProvider CreateProvider()
        {
            // Ideally, the enlightenment dll name is the same as the PCL dll, with all other attributes matching.
            var enlightenmentAssemblyName = new AssemblyName(ReflectionShim.GetAssembly(typeof(IEnlightenmentProvider)).FullName)
            {
                Name = "Nito.AsyncEx.Enlightenment",
            };
            
            var enlightenmentProviderType = TryLoadAssemblyAndGetType("Nito.AsyncEx.Internal.PlatformEnlightenment.EnlightenmentProvider, " + enlightenmentAssemblyName.FullName);
            if (enlightenmentProviderType == null)
            {
                // However, some platforms (i.e., Windows Phone) don't support signing though they do enforce signatures.
                // So we also search for an enlightenment dll that has all attributes matching except the public key token.
                enlightenmentAssemblyName.SetPublicKeyToken(null);
                enlightenmentProviderType = TryLoadAssemblyAndGetType("Nito.AsyncEx.Internal.PlatformEnlightenment.EnlightenmentProvider, " + enlightenmentAssemblyName.FullName);
            }
            if (enlightenmentProviderType == null)
                return DefaultProvider;

            return (IEnlightenmentProvider)Activator.CreateInstance(enlightenmentProviderType);
        }

        /// <summary>
        /// Attempts to load a type that is expected to be in another assembly. Returns <c>null</c> if the type could not be found or the assembly could not be loaded.
        /// </summary>
        /// <param name="typeName">The full name of the type to load.</param>
        private static Type TryLoadAssemblyAndGetType(string typeName)
        {
            try
            {
                return Type.GetType(typeName, false);
            }
            catch (IOException)
            {
                return null;
            }
            catch (BadImageFormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates an enlightenment using the platform enlightenment provider; falls back to the default enlightenment provider if the platform provider returns <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The type of enlightenment to create.</typeparam>
        private static T CreateEnlightenment<T>() where T : class
        {
            return Platform.CreateEnlightenment<T>() ?? DefaultProvider.CreateEnlightenment<T>();
        }

        /// <summary>
        /// Returns the platform enlightenment provider, if it could be found; otherwise, returns the default enlightenment provider.
        /// </summary>
        public static IEnlightenmentProvider Platform
        {
            get
            {
                if (_platform == null)
                    Interlocked.CompareExchange(ref _platform, CreateProvider(), null);
                return _platform;
            }
        }

        /// <summary>
        /// Returns the exception enlightenment.
        /// </summary>
        public static IExceptionEnlightenment Exception
        {
            get
            {
                if (_exception == null)
                    Interlocked.CompareExchange(ref _exception, CreateEnlightenment<IExceptionEnlightenment>(), null);
                return _exception;
            }
        }

        /// <summary>
        /// Returns the async enlightenment.
        /// </summary>
        public static IAsyncEnlightenment Async
        {
            get
            {
                if (_async == null)
                    Interlocked.CompareExchange(ref _async, CreateEnlightenment<IAsyncEnlightenment>(), null);
                return _async;
            }
        }

        /// <summary>
        /// Returns the synchronization context enlightenment.
        /// </summary>
        public static ISynchronizationContextEnlightenment SynchronizationContext
        {
            get
            {
                if (_synchronizationContext == null)
                    Interlocked.CompareExchange(ref _synchronizationContext, CreateEnlightenment<ISynchronizationContextEnlightenment>(), null);
                return _synchronizationContext;
            }
        }

        /// <summary>
        /// Returns the trace enlightenment.
        /// </summary>
        public static ITraceEnlightenment Trace
        {
            get
            {
                if (_trace == null)
                    Interlocked.CompareExchange(ref _trace, CreateEnlightenment<ITraceEnlightenment>(), null);
                return _trace;
            }
        }

        /// <summary>
        /// Returns the concurrent collections enlightenment.
        /// </summary>
        public static IConcurrentCollectionsEnlightenment ConcurrentCollections
        {
            get
            {
                if (_concurrentCollections == null)
                    Interlocked.CompareExchange(ref _concurrentCollections, CreateEnlightenment<IConcurrentCollectionsEnlightenment>(), null);
                return _concurrentCollections;
            }
        }

        /// <summary>
        /// Returns the lazy enlightenment.
        /// </summary>
        public static ILazyEnlightenment Lazy
        {
            get
            {
                if (_lazy == null)
                    Interlocked.CompareExchange(ref _lazy, CreateEnlightenment<ILazyEnlightenment>(), null);
                return _lazy;
            }
        }

        /// <summary>
        /// Returns the thread pool enlightenment.
        /// </summary>
        public static IThreadPoolEnlightenment ThreadPool
        {
            get
            {
                if (_threadPool == null)
                    Interlocked.CompareExchange(ref _threadPool, CreateEnlightenment<IThreadPoolEnlightenment>(), null);
                return _threadPool;
            }
        }

        /// <summary>
        /// Returns the thread identity enlightenment.
        /// </summary>
        public static IThreadIdentityEnlightenment ThreadIdentity
        {
            get
            {
                if (_threadIdentity == null)
                    Interlocked.CompareExchange(ref _threadIdentity, CreateEnlightenment<IThreadIdentityEnlightenment>(), null);
                return _threadIdentity;
            }
        }

        /// <summary>
        /// Returns the STA enlightenment.
        /// </summary>
        public static ISingleThreadedApartmentEnlightenment SingleThreadedApartment
        {
            get
            {
                if (_singleThreadedApartment == null)
                    Interlocked.CompareExchange(ref _singleThreadedApartment, CreateEnlightenment<ISingleThreadedApartmentEnlightenment>(), null);
                return _singleThreadedApartment;
            }
        }
    }
}
