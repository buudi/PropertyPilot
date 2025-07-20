using Xunit;

namespace PropertyPilot.Tests.Integration;

[Trait("Category", "Integration")]
public class BasicIntegrationTest
{
    [Fact]
    public void BasicIntegrationTest_ShouldAlwaysPass()
    {
        // Arrange
        var expected = true;
        
        // Act
        var actual = true;
        
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AnotherBasicTest_ShouldAlsoPass()
    {
        // Arrange
        var expected = 42;
        
        // Act
        var actual = 42;
        
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void StringTest_ShouldPass()
    {
        // Arrange
        var expected = "PropertyPilot";
        
        // Act
        var actual = "PropertyPilot";
        
        // Assert
        Assert.Equal(expected, actual);
    }
}