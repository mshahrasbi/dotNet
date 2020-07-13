using System.Threading.Tasks;

namespace WebAdvert.Web.ServiceClients
{
    public interface IAdvertApiCleint
    {
        Task<AdvertResponse> Create(CreateAdvertModel model);
    }
}
