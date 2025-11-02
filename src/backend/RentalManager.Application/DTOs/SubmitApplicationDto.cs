// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Application.DTOs;

public class SubmitApplicationDto
{
    public Guid PropertyId { get; set; }

    public ApplicationDataDto ApplicationData { get; set; } = default!;
}