// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit
{
    using System;
    using Audit;
    using Audit.MetadataFactories;
    using Audit.Observers;
    using Configurators;
    using Pipeline;


    public static class AuditConfigurationExtensions
    {
        /// <summary>
        /// Adds observers that will audit all published and sent messages, sending them to the message audit store after they are sent/published.
        /// </summary>
        /// <param name="connector">The bus</param>
        /// <param name="store">Audit store</param>
        /// <param name="configureFilter">Filter configuration delegate</param>
        /// <param name="metadataFactory">Message metadata factory. If omited, the default one will be used.</param>
        public static void ConnectSendAuditObservers<T>(this T connector, IMessageAuditStore store, Action<IMessageFilterConfigurator> configureFilter = null,
            ISendMetadataFactory metadataFactory = null)
            where T : ISendObserverConnector, IPublishObserverConnector
        {
            var specification = new SendMessageFilterSpecification();
            configureFilter?.Invoke(specification);

            var factory = metadataFactory ?? new DefaultSendMetadataFactory();

            connector.ConnectSendObserver(new AuditSendObserver(store, factory, specification.Filter));
            connector.ConnectPublishObserver(new AuditPublishObserver(store, factory, specification.Filter));
        }

        /// <summary>
        /// Add an observer that will audit consumed messages, sending them to the message audit store prior to consumption by the consumer
        /// </summary>
        /// <param name="connector">The bus or endpoint</param>
        /// <param name="store">The audit store</param>
        /// <param name="configureFilter">Filter configuration delegate</param>
        /// <param name="metadataFactory">Message metadata factory. If omited, the default one will be used.</param>
        public static void ConnectConsumeAuditObserver(this IConsumeObserverConnector connector, IMessageAuditStore store,
            Action<IMessageFilterConfigurator> configureFilter = null, IConsumeMetadataFactory metadataFactory = null)
        {
            if (connector == null)
                throw new ArgumentNullException(nameof(connector));
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var specification = new ConsumeMessageFilterSpecification();
            configureFilter?.Invoke(specification);

            var factory = metadataFactory ?? new DefaultConsumeMetadataFactory();

            connector.ConnectConsumeObserver(new AuditConsumeObserver(store, factory, specification.Filter));
        }
    }
}