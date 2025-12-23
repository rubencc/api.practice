using Api.Weather.Host.Resources;
using Api.Weather.Host.Validations;
using AwesomeAssertions;

namespace Api.Weather.Host.UnitTest;

public class PostalCodeValidationTests
{
    private PostalCodeValidation postalCodeValidation;

    public PostalCodeValidationTests()
    {
        this.postalCodeValidation = new PostalCodeValidation();
    }
    
    [Theory]
    [InlineData("08001", true)]
    [InlineData("00001", false)]
    public void Validate_postal_codes(string value, bool expected)
    {
        // Arrange
        this.postalCodeValidation = new PostalCodeValidation();
        var request = new ForecastRequest
        {
            PostalCode = value
        };

        // Act
        var result = this.postalCodeValidation.IsValid(request);

        // Assert
        result.Should().Be(expected);
    }

}