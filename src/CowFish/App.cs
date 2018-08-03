using System;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Timers;
using Autofac;
using CowFish.Core;
using CowFish.Core.Common.Bootstrap;
using MediatR;
using NLog;
using Topshelf;
using Topshelf.Autofac;

namespace CowFish
{
    internal class App
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();

        }
        private static async Task MainAsync(string[] args)
        {
            Boot.Instance.Start(typeof(App).Assembly);
            Boot.Instance.AddAssembly(typeof(Consts).Assembly, AssemblyInProject.Core);


            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(Boot.Instance.GetAssemblies());
            try
            {
                using (var container = builder.Build())
                {
                    using (var scope = container.BeginLifetimeScope())
                    {
                        _log.Info($"Aplication: {Boot.Instance.GetAppEnvironment().AppVersion.FullName}");
                        var mediator = scope.Resolve<IMediator>();
                        // Any configuration checks, initializations should be handled by this event
                        // Most important start code is in the AppHandlers class. 
                        // You may extend it as you wont. 
                        await mediator.Publish(new AppStartingEvent());

                        // And after 
                        await mediator.Publish(new AppStartedEvent());

                        var rc = HostFactory.Run(x =>                                   //1
                        {

                            x.UseAutofacContainer(scope);

                            x.Service<TownCrier>(s =>                                   //2
                            {
                                // Let Topshelf use it
                                //s.ConstructUsingAutofacContainer();

                                s.ConstructUsing(name => new TownCrier());                //3
                                s.WhenStarted(tc => tc.Start());                         //4
                                s.WhenStopped(tc => tc.Stop());                          //5
                            });
                            x.RunAsLocalSystem();                                       //6

                            x.SetDescription(Consts.SERVICE_DESCRIPTION);                   //7
                            x.SetDisplayName(Consts.SERVICE_DISPLAY_NAME);                                  //8
                            x.SetServiceName(Consts.SERVICE_NAME);                                  //9
                        });                                                             //10

                        var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  //11
                        Environment.ExitCode = exitCode;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                _log.Error(e.Message);
                _log.Error(e.InnerException?.Message);
              
                throw e;
            }
        }


    }

    public class TownCrier
    {
        readonly Timer _timer;
        public TownCrier()
        {
            _timer = new Timer(1000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) => Console.WriteLine($"It is {DateTime.Now} and all is well");
        }
        public void Start() { _timer.Start(); }
        public void Stop() { _timer.Stop(); }
    }

}