namespace Api.Practice.UnitTest;

using Api.Practice.Resources;
using Api.Practice.Validations;
using FluentAssertions;

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