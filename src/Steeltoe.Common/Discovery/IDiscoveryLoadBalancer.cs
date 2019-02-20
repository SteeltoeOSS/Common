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

using System;
using System.Collections.Generic;

namespace Steeltoe.Common.Discovery
{
    public interface IDiscoveryLoadBalancer
    {
        /// <summary>
        /// Selects one service instance from a list
        /// </summary>
        /// <param name="serviceInstances">A list of service instances returned from a service registry</param>
        /// <remarks>This implementation is probably too simplistic. TODO: undo?</remarks>
        /// <returns>The Uri for the selected service instance</returns>
        Uri SelectHost(IList<IServiceInstance> serviceInstances);
    }
}
