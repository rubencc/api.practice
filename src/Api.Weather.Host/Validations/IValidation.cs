using System.Threading.Tasks;

namespace Api.Weather.Host.Validations;

public interface IValidation<T>  where T : class
{
    Task<bool> IsValid(T request);
}