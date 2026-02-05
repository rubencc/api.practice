---
description: 'Best practices auto unit test with xunit'
applyTo: 'tests classes'
scope: 'Only for .net projects'
---

# Unit Test Generation Instructions for .NET with xUnit

## Role
You are an expert .NET developer specializing in creating comprehensive unit tests. Your goal is to generate high-quality, maintainable unit tests that maximize code coverage.

## Testing Framework Stack
- **Test Framework**: xUnit
- **Mocking Library**: NSubstitute
- **Assertion Library**: Awesome Assertions (similar to FluentAssertions)

## Core Principles

### 1. Test Class Management
- **Add Only**: Only add new test methods to existing test classes. Never modify or remove existing tests.
- **Naming Convention**: If the test class doesn't exist, create it using the pattern `{ClassUnderTest}Tests`
    - Example: For class `UserService`, create `UserServiceTests`
- **Single Target**: Only create tests for the specified class. Do not modify or create tests for other classes.

### 2. Code Coverage Objectives
Your tests must achieve maximum code coverage by:
- **Parameter Combinations**: Test all meaningful combinations of input parameters
- **Code Branches**: Test all conditional branches (if/else, switch cases, loops)
- **Edge Cases**: Test boundary conditions, null values, empty collections, etc.
- **Exception Paths**: Test both happy paths and error scenarios

### 3. Test Method Requirements
- **Review Comments**: Add a comment `// TODO: Review this test` at the beginning of each generated test method
- **Theory Usage**: When testing parameter combinations, prefer xUnit `[Theory]` with `[InlineData]` or `[MemberData]` over multiple `[Fact]` methods
- **Descriptive Names**: Use clear, descriptive method names that explain what is being tested
    - Pattern: `MethodName_Scenario_ExpectedBehavior`
    - Example: `CreateUser_WithNullEmail_ThrowsArgumentNullException`

## Test Structure Template

```csharp
public class {ClassName}Tests
{
    // TODO: Review this test
    [Fact]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var dependency = Substitute.For<IDependency>();
        var sut = new ClassUnderTest(dependency);
        
        // Act
        var result = sut.MethodToTest(parameters);
        
        // Assert
        result.Should().Be(expectedValue);
    }
    
    // TODO: Review this test
    [Theory]
    [InlineData(param1, param2, expectedResult)]
    [InlineData(param1, param2, expectedResult)]
    public void MethodName_VariousInputs_ReturnsExpectedResults(
        type param1, type param2, type expected)
    {
        // Arrange
        var sut = new ClassUnderTest();
        
        // Act
        var result = sut.MethodToTest(param1, param2);
        
        // Assert
        result.Should().Be(expected);
    }
}
```

## Coverage Guidelines

### Parameter Combination Testing
For methods with multiple parameters, create theory-based tests:
- Test valid combinations
- Test invalid combinations
- Test boundary values (min, max, zero, negative)
- Test null/empty values where applicable

### Branch Coverage
Ensure all code paths are tested:
- All `if`/`else` branches
- All `switch` cases including `default`
- Early returns
- Loop conditions (zero iterations, one iteration, many iterations)
- Try-catch blocks

### Dependency Interactions
When testing classes with dependencies:
- Mock all external dependencies using NSubstitute
- Verify that dependencies are called correctly
- Test different return values from dependencies
- Test exception handling when dependencies throw

## Assertion Examples with Awesome Assertions

```csharp
// Value assertions
result.Should().Be(expected);
result.Should().NotBe(unexpected);

// Null checks
result.Should().NotBeNull();
result.Should().BeNull();

// Collection assertions
collection.Should().HaveCount(5);
collection.Should().Contain(item);
collection.Should().BeEmpty();

// String assertions
text.Should().Contain("substring");
text.Should().StartWith("prefix");
text.Should().BeNullOrEmpty();

// Exception assertions
Action act = () => sut.MethodThatThrows();
act.Should().Throw<ArgumentNullException>()
   .WithMessage("*parameter*");

// Boolean assertions
result.Should().BeTrue();
result.Should().BeFalse();

// Type assertions
result.Should().BeOfType<ExpectedType>();
result.Should().BeAssignableTo<IInterface>();
```

## NSubstitute Mock Examples

This is the documentation for NSubstitute mocking framework: https://nsubstitute.github.io/help/getting-started/index.html

```csharp
// Create a mock
var mock = Substitute.For<IInterface>();

// Setup return values
mock.Method(Arg.Any<string>()).Returns(x => "value");
mock.Method("specific").Returns(x => "result");

//Setup for null values 
mock.Method("specific").ReturnsNull();

// Verify calls
mock.Received(1).Method(Arg.Any<string>());
mock.DidNotReceive().OtherMethod();

// Setup exceptions
mock.Method(Arg.Any<string>()).Throws(new Exception("error"));

// Argument matching
mock.Method(Arg.Is<string>(x => x.Length > 5)).Returns("long");
```

## Extra instructions when ReturnsNull is used

It's important to include the following using directive at the top of your test file when utilizing the `ReturnsNull` feature from NSubstitute:

```csharp
using NSubstitute.ReturnsExtensions;
```

## Workflow

1. **Analyze the Target Class**
    - Identify all public methods
    - Identify all parameters and their types
    - Identify all code branches and conditions
    - Identify all dependencies

2. **Check for Existing Test Class**
    - If exists: append new tests only
    - If not: create new test class with proper naming

3. **Generate Test Methods**
    - Create theory-based tests for parameter combinations
    - Create fact-based tests for specific scenarios
    - Add `// TODO: Review this test` comment to each method
    - Use Arrange-Act-Assert pattern consistently

4. **Ensure Maximum Coverage**
    - Verify all public methods have tests
    - Verify all branches are covered
    - Verify edge cases are included
    - Verify exception scenarios are tested

## Important Reminders
- ✅ Only ADD new test methods
- ❌ Never MODIFY existing test methods
- ❌ Never DELETE existing test methods
- ✅ Always add review comments
- ✅ Focus only on the specified class
- ✅ Use Theory for parameter combinations
- ✅ Use NSubstitute for mocking
- ✅ Use Awesome Assertions for all assertions
- ✅ Follow Arrange-Act-Assert pattern
- ✅ Use descriptive test method names

## Input Required
When using these instructions, provide:
- **Class Name**: The full name of the class to test (including namespace if needed)
- **Source Code**: The implementation of the class to be tested
- **Dependencies**: Any interfaces or classes that the target class depends on

## Output Expected
- A complete test class with comprehensive unit tests
- Maximum code coverage through parameter combinations and branch testing
- All new tests marked with review comments
- Proper use of xUnit, NSubstitute, and Awesome Assertions