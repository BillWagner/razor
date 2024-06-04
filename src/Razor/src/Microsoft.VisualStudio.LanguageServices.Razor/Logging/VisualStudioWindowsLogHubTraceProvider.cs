﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.LogHub;
using Microsoft.VisualStudio.RpcContracts.Logging;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.Razor.Logging;

[Export(typeof(RazorLogHubTraceProvider))]
internal class VisualStudioWindowsLogHubTraceProvider : RazorLogHubTraceProvider
{
    // SemaphoreSlim is banned. See https://github.com/dotnet/razor/issues/10390 for more info.
#pragma warning disable RS0030 // Do not use banned APIs

    private static readonly LoggerOptions s_logOptions = new(
        requestedLoggingLevel: new LoggingLevelSettings(SourceLevels.Information | SourceLevels.ActivityTracing),
        privacySetting: PrivacyFlags.MayContainPersonallyIdentifibleInformation | PrivacyFlags.MayContainPrivateInformation);

    private readonly IVsService<SVsBrokeredServiceContainer, IBrokeredServiceContainer> _brokeredServiceContainer;
    private readonly ReentrantSemaphore _initializationSemaphore;

    private IServiceBroker? _serviceBroker = null;
    private TraceSource? _traceSource;

    [ImportingConstructor]
    public VisualStudioWindowsLogHubTraceProvider(
        IVsService<SVsBrokeredServiceContainer, IBrokeredServiceContainer> brokeredServiceContainer,
        JoinableTaskContext joinableTaskContext)
    {
        _brokeredServiceContainer = brokeredServiceContainer;
        _initializationSemaphore = ReentrantSemaphore.Create(initialCount: 1, joinableTaskContext, ReentrantSemaphore.ReentrancyMode.NotAllowed);
    }

    public override async Task InitializeTraceAsync(string logIdentifier, int logHubSessionId, CancellationToken cancellationToken)
    {
        var serviceBrokerInitialized = await TryInitializeServiceBrokerAsync(cancellationToken).ConfigureAwait(false);
        if (!serviceBrokerInitialized)
        {
            return;
        }

        var serviceBroker = _serviceBroker.AssumeNotNull();

        var logId = new LogId(
            logName: $"{logIdentifier}.{logHubSessionId}",
            serviceId: new ServiceMoniker($"Razor.{logIdentifier}"));

        using var traceConfig = await TraceConfiguration
            .CreateTraceConfigurationInstanceAsync(serviceBroker, ownsServiceBroker: true, cancellationToken)
            .ConfigureAwait(false);

        _traceSource = await traceConfig.RegisterLogSourceAsync(logId, s_logOptions, cancellationToken).ConfigureAwait(false);
    }

    public override TraceSource? TryGetTraceSource()
    {
        return _traceSource;
    }

    private async Task<bool> TryInitializeServiceBrokerAsync(CancellationToken cancellationToken)
    {
        // Check if the service broker has already been initialized
        if (_serviceBroker is not null)
        {
            return true;
        }

        await _initializationSemaphore.ExecuteAsync(async () =>
        {
            if (_serviceBroker is null &&
                await _brokeredServiceContainer.GetValueOrNullAsync(cancellationToken) is IBrokeredServiceContainer serviceContainer)
            {
                _serviceBroker = serviceContainer.GetFullAccessServiceBroker();
            }
        },
        cancellationToken);

        return _serviceBroker is not null;
    }
}
