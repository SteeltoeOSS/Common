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
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Common.Discovery;
using Steeltoe.Common.Http.Test;
using System;
using System.Collections.Generic;
using Xunit;

namespace Steeltoe.Common.Http.LoadBalancer.Test
{
    public class RoundRobinDistributedLoadBalancerTest
    {
        [Fact]
        public void Throws_If_IServiceInstanceProviderNotProvided()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new RoundRobinDistributedLoadBalancer(null, GetCache()));
            Assert.Equal("serviceInstanceProvider", exception.ParamName);
        }

        [Fact]
        public void Throws_If_IDistributedNotProvided()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new RoundRobinDistributedLoadBalancer(new ConfigurationServiceInstanceProvider(new TestOptionsMonitor<List<ConfigurationServiceInstance>>(new List<ConfigurationServiceInstance>())), null));
            Assert.Equal("distributedCache", exception.ParamName);
        }

        [Fact]
        public async void ResolveServiceInstance_ResolvesAndIncrementsServiceIndex()
        {
            // arrange
            var services = new List<ConfigurationServiceInstance>
            {
                new ConfigurationServiceInstance { ServiceId = "fruitservice", Host = "fruitball", Port = 8000, IsSecure = true },
                new ConfigurationServiceInstance { ServiceId = "fruitservice", Host = "fruitballer", Port = 8001 },
                new ConfigurationServiceInstance { ServiceId = "fruitservice", Host = "fruitballerz", Port = 8002 },
                new ConfigurationServiceInstance { ServiceId = "vegetableservice", Host = "vegemite", Port = 8010, IsSecure = true },
                new ConfigurationServiceInstance { ServiceId = "vegetableservice", Host = "carrot", Port = 8011 },
                new ConfigurationServiceInstance { ServiceId = "vegetableservice", Host = "beet", Port = 8012 },
            };
            var serviceOptions = new TestOptionsMonitor<List<ConfigurationServiceInstance>>(services);
            var provider = new ConfigurationServiceInstanceProvider(serviceOptions);
            var loadBalancer = new RoundRobinDistributedLoadBalancer(provider, GetCache());

            // act
            var fruitResult = await loadBalancer.ResolveServiceInstanceAsync(new Uri("http://fruitservice/api"));
            await loadBalancer.ResolveServiceInstanceAsync(new Uri("http://vegetableservice/api"));
            var vegResult = await loadBalancer.ResolveServiceInstanceAsync(new Uri("http://vegetableservice/api"));

            // assert
            Assert.Equal(8000, fruitResult.Port);
            Assert.Equal(8011, vegResult.Port);
        }

        private IDistributedCache GetCache()
        {
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetService<IDistributedCache>();
        }
    }
}
