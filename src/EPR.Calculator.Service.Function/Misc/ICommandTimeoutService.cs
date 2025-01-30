using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ICommandTimeoutService
    {
        void SetCommandTimeout(DatabaseFacade database, string key);
    }
}