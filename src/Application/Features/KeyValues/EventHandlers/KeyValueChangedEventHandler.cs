// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Events;
using CleanArchitecture.Blazor.Domain.Features.KeyValues;

namespace CleanArchitecture.Blazor.Application.Features.KeyValues.EventHandlers;

public class KeyValueChangedEventHandler : INotificationHandler<UpdatedEvent<KeyValue>>
{
    private readonly ILogger<KeyValueChangedEventHandler> _logger;
    private readonly IPicklistService _picklistService;

    public KeyValueChangedEventHandler(
        IPicklistService picklistService,
        ILogger<KeyValueChangedEventHandler> logger
    )
    {
        _picklistService = picklistService;
        _logger = logger;
    }

    public async Task Handle(UpdatedEvent<KeyValue> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("KeyValue Changed {DomainEvent},{@Entity}", nameof(notification), notification.Entity);
        await _picklistService.Refresh();
    }
}