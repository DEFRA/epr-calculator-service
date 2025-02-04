﻿namespace EPR.Calculator.Service.Function.Interface
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IRpdStatusService
    {
        Task<TimeSpan?> UpdateRpdStatus(int runId, string updatedBy, bool isPomSuccessful, CancellationToken timeout);
    }
}