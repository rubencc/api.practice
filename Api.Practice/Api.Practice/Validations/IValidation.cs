namespace Api.Practice.Validations;

using System.Threading.Tasks;

public interface IValidation<T>  where T : class
{
    Task<bool> IsValid(T request);
}