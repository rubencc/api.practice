namespace Weather.Application.Interfaces;

public interface IValidation<T>  where T : class
{
    Task<bool> IsValid(T command);
}