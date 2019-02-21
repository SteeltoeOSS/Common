// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Logging;
using Steeltoe.Common.Discovery;
using Steeltoe.Common.LoadBalancer;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Steeltoe.Common.Http.LoadBalancer
{
    public class RoundRobinLoadBalancer : ILoadBalancer
    {
        internal readonly IServiceInstanceProvider ServiceInstanceProvider;
        internal readonly ConcurrentDictionary<string, int> NextIndexForService = new ConcurrentDictionary<string, int>();
        private readonly ILogger _logger;

        public RoundRobinLoadBalancer(IServiceInstanceProvider serviceInstanceProvider, ILogger logger = null)
        {
            ServiceInstanceProvider = serviceInstanceProvider ?? throw new ArgumentNullException(nameof(serviceInstanceProvider));
            _logger = logger;
        }

        public async Task<Uri> ResolveServiceInstanceAsync(Uri request)
        {
            var serviceName = request.Host;
            _logger?.LogTrace("ResolveServiceInstance {serviceName}", serviceName);

            // get instances for this service
            var availableServiceInstances = await Task.Run(() => ServiceInstanceProvider.GetInstances(serviceName));

            // get next instance, or wrap back to first instance if we reach the end of the list
            IServiceInstance serviceInstance = null;
            var nextInstanceIndex = NextIndexForService.GetOrAdd(serviceName, 0);
            if (nextInstanceIndex >= availableServiceInstances.Count)
            {
                serviceInstance = availableServiceInstances[0];
                NextIndexForService[serviceName] = 1;
            }
            else
            {
                serviceInstance = availableServiceInstances[nextInstanceIndex];
                NextIndexForService[serviceName] += 1;
            }

            return new Uri(serviceInstance.Uri, request.PathAndQuery);
        }

        public Task UpdateStatsAsync(Uri originalUri, Uri resolvedUri, TimeSpan responseTime, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}
