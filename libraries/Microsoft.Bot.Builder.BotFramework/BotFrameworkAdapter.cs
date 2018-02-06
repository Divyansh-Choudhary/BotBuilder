﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.BotFramework
{
    public class BotFrameworkAdapter : ActivityAdapterBase
    {
        private readonly SimpleCredentialProvider _credentialProvider;
        private readonly MicrosoftAppCredentials _credentials;

        public BotFrameworkAdapter(IConfiguration configuration) : base()
        {
            _credentialProvider = new ConfigurationCredentialProvider(configuration);
            _credentials = new MicrosoftAppCredentials(this._credentialProvider.AppId, _credentialProvider.Password);                       
        }
        public BotFrameworkAdapter(string appId, string appPassword) : base()
        {
            _credentials = new MicrosoftAppCredentials(appId, appPassword);
            _credentialProvider = new SimpleCredentialProvider(appId, appPassword);
        }

        public async override Task Post(IList<IActivity> activities)
        {
            BotAssert.ActivityListNotNull(activities);

            foreach (Activity activity in activities)
            {
                if (activity.Type == "delay")
                {
                    // The Activity Schema doesn't have a delay type build in, so it's simulated
                    // here in the Bot. This matches the behavior in the Node connector. 
                    int delayMs = (int)activity.Value;
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
                else
                {
                    var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), _credentials);
                    await connectorClient.Conversations.SendToConversationAsync(activity).ConfigureAwait(false);
                }
            }
        }

        public async Task Receive(string authHeader, Activity activity)
        {
            BotAssert.ActivityNotNull(activity);

            await JwtTokenValidation.AssertValidActivity(activity, authHeader, _credentialProvider);

            if (this.OnReceive != null)
            {
                await this.OnReceive(activity).ConfigureAwait(false);
            }
        }
    }
}
