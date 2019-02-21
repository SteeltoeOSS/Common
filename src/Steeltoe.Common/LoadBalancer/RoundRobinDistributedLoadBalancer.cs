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

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Steeltoe.Common.Discovery;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Common.LoadBalancer
{
    public class RoundRobinDistributedLoadBalancer : ILoadBalancer
    {
        internal readonly IServiceInstanceProvider ServiceInstanceProvider;
        internal readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;

        public RoundRobinDistributedLoadBalancer(IServiceInstanceProvider serviceInstanceProvider, IDistributedCache distributedCache, ILogger logger = null)
        {
            ServiceInstanceProvider = serviceInstanceProvider ?? throw new ArgumentNullException(nameof(serviceInstanceProvider));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger;
        }

        public async Task<Uri> ResolveServiceInstanceAsync(Uri request)
        {
            var serviceName = request.Host;
            _logger?.LogTrace("ResolveServiceInstance {serviceName}", serviceName);
            string cacheKey = serviceName + "LoadBalancerIndex";

            // get instances for this service
            var availableServiceInstances = await Task.Run(() => ServiceInstanceProvider.GetInstances(serviceName));
            if (!availableServiceInstances.Any())
            {
                _logger?.LogError("No service instances available for {serviceName}", serviceName);
                return request;
            }

            // get next instance, or wrap back to first instance if we reach the end of the list
            IServiceInstance serviceInstance = null;
            var nextInstanceIndex = await GetOrInitNextIndex(cacheKey, 0);
            if (nextInstanceIndex >= availableServiceInstances.Count)
            {
                nextInstanceIndex = 0;
            }

            serviceInstance = availableServiceInstances[nextInstanceIndex];
            await _distributedCache.SetAsync(cacheKey, BitConverter.GetBytes(nextInstanceIndex + 1));
            return new Uri(serviceInstance.Uri, request.PathAndQuery);
        }

        public Task UpdateStatsAsync(Uri originalUri, Uri resolvedUri, TimeSpan responseTime, Exception exception)
        {
            return Task.CompletedTask;
        }

        private async Task<int> GetOrInitNextIndex(string cacheKey, int initValue)
        {
            int index = initValue;
            var cacheEntry = await _distributedCache.GetAsync(cacheKey);
            if (cacheEntry != null && cacheEntry.Length > 0)
            {
                index = BitConverter.ToInt16(cacheEntry, 0);
            }

            return index;
        }
    }
}