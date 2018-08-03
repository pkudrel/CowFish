using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using NLog;

namespace CowFish.Core.Common.Bootstrap
{
    public class Boot
    {
        private static readonly List<string> _buffer = new List<string>();
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static readonly Boot _instance = new Boot();
        private AppEnvironment _appEnvironment;
	    private ILifetimeScope _scope;


		static Boot()
        {
        }

        private Boot()
        {
        }

        // ReSharper disable once ConvertToAutoProperty
        public static Boot Instance => _instance;


	    public void SetScope(ILifetimeScope scope)
	    {
		    _scope = scope;

	    }
        public void Start(Assembly mainAssembly)
        {
            _buffer.Add($"Begin boot; Main assembly: {mainAssembly.GetName().Name}");
            AssemblyCollector.Instance.AddAssembly(mainAssembly, AssemblyInProject.Main);
            _appEnvironment = AppEnvironmentBuilder.Instance.GetAppEnvironment(mainAssembly);
            // Configure Nlog as soon as possible
            DeveloperMode.NLog(_appEnvironment, _buffer);
            FlushBuffer(_buffer);
            _log.Debug("After NLog config");
        }

        public AppEnvironment GetAppEnvironment()
        {
            if (_appEnvironment == null)
                throw new NullReferenceException("Application has not been started correctly. Use Start method");
            return _appEnvironment;
        }

        private static void FlushBuffer(ICollection<string> list)
        {
            foreach (var msg in list) _log.Debug(msg);
            list.Clear();
        }

        public void AddAssembly(Assembly assembly, AssemblyInProject assemblyInProject)
        {
            AssemblyCollector.Instance.AddAssembly(assembly, assemblyInProject);
        }

        public Assembly[] GetAssemblies()
        {
            return AssemblyCollector.Instance.GetAssemblies();
        }

	    public Func<Type, string, IEnumerable<object>> GetProducerFunction()
	    {
			Func<Type, string, IEnumerable<object>> getAllServices = (service, contract) =>
			{
				_log.Debug($"GET: {service.Name}");
				var enumerableType = typeof(IEnumerable<>).MakeGenericType(service);
				var instance = string.IsNullOrEmpty(contract)
					? _scope.Resolve(enumerableType)
					: _scope.ResolveNamed(contract, enumerableType);
				return ((IEnumerable)instance).Cast<object>();
			};

		    return getAllServices;
	    }
    }
}