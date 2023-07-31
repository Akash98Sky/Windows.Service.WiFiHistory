using System.Threading.Tasks;

namespace WiFiHistory.Service.Interfaces
{
    public interface IHelperService
    {
        Task PerformService(string schedule);
    }
}
