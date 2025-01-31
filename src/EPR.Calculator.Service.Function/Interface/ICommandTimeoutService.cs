using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface ICommandTimeoutService
    {
        void SetCommandTimeout(DatabaseFacade database, string key);
    }
}