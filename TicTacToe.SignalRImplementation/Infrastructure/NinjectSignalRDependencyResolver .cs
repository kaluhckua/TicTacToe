using Microsoft.AspNet.SignalR;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicTacToe.SignalRImplementation.Infrastructure
{
    public class NinjectSignalRDependencyResolver : DefaultDependencyResolver
    {
       
            private readonly IKernel _kernel;

            public NinjectSignalRDependencyResolver(IKernel kernel)
            {
                if (kernel == null)
                {
                    throw new ArgumentNullException("kernel");
                }

                _kernel = kernel;
            }

            public override object GetService(Type serviceType)
            {
                var service = _kernel.TryGet(serviceType) ?? base.GetService(serviceType);
                return service;
            }

            public override IEnumerable<object> GetServices(Type serviceType)
            {
                var services = _kernel.GetAll(serviceType).Concat(base.GetServices(serviceType));
                return services;
            }
        }
    }

