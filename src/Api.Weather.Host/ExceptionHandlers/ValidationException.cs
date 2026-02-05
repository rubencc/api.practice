using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Api.Weather.Host.ExceptionHandlers;

public class ValidationException : Exception
{
    private IDictionary<string, string[]> errors;
    
    public IDictionary<string, string[]> Errors => errors;
    
    public ValidationException(string message, List<ValidationFailure> failures) : base(message)
    {
        errors = failures
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

    }

    public ValidationException(string message, Exception innerException, List<ValidationFailure> failures) 
        : base(message, innerException)
    {
        errors = failures
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
    }
}
