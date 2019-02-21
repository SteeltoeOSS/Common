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

using Steeltoe.Common.Discovery;
using Steeltoe.Common.Http.LoadBalancer;
using Steeltoe.Common.LoadBalancer;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LoadBalancerHttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="DelegatingHandler"/> that performs random load balancing
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <remarks>You also need to either manually configure service instances in app config or add an <see cref="IServiceInstanceProvider" /> or <see cref="IDiscoveryClient"/> to the DI container so the load balancer can sent traffic to more than one address</remarks>
        /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
        public static IHttpClientBuilder AddRandomLoadBalancer(this IHttpClientBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!builder.Services.Contains(new ServiceDescriptor(typeof(ILoadBalancer), typeof(RandomLoadBalancer), ServiceLifetime.Transient)))
            {
                builder.Services.AddTransient(typeof(ILoadBalancer), typeof(RandomLoadBalancer));
                builder.Services.AddTransient(typeof(RandomLoadBalancer), typeof(RandomLoadBalancer));
            }

            return builder.AddLoadBalancer<RandomLoadBalancer>();
        }

        /// <summary>
        /// Adds a <see cref="DelegatingHandler"/> that performs round robin load balancing
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <remarks>You also need to either manually configure service instances in app config or add an <see cref="IServiceInstanceProvider" /> or <see cref="IDiscoveryClient"/> to the DI container so the load balancer can sent traffic to more than one address</remarks>
        /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
        public static IHttpClientBuilder AddRoundRobinLoadBalancer(this IHttpClientBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!builder.Services.Contains(new ServiceDescriptor(typeof(ILoadBalancer), typeof(RoundRobinLoadBalancer), ServiceLifetime.Transient)))
            {
                builder.Services.AddTransient(typeof(ILoadBalancer), typeof(RoundRobinLoadBalancer));
                builder.Services.AddTransient(typeof(RoundRobinLoadBalancer), typeof(RoundRobinLoadBalancer));
            }

            return builder.AddLoadBalancer<RoundRobinLoadBalancer>();
        }

        /// <summary>
        /// Adds an <see cref="HttpMessageHandler"/> with specified load balancer
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <typeparam name="T">The type of <see cref="ILoadBalancer"/> to use</typeparam>
        /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
        public static IHttpClientBuilder AddLoadBalancer<T>(this IHttpClientBuilder builder)
            where T : ILoadBalancer
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!builder.Services.Contains(new ServiceDescriptor(typeof(LoadBalancerDelegatingHandler<T>), typeof(LoadBalancerDelegatingHandler<T>), ServiceLifetime.Transient)))
            {
                builder.Services.AddTransient<LoadBalancerDelegatingHandler<T>>();
            }

            builder.AddHttpMessageHandler<LoadBalancerDelegatingHandler<T>>();
            return builder;
        }
    }
}
