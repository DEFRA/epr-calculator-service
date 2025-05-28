using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface ICommandTimeoutService
    {
        void SetCommandTimeout(DatabaseFacade database);
    }
}