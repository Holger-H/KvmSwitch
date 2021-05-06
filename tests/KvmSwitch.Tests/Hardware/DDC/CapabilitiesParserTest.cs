namespace KvmSwitch.Tests.Hardware.DDC
{
    using System.Linq;
    using FluentAssertions;
    using KvmSwitch.Hardware.DDC;
    using Xunit;

    public class CapabilitiesParserTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("()")]
        [InlineData("(  )")]
        public void ParseCapabilities_Returns_Empty_List_On_Empty_Input2(string input)
        {
            var displayCapability = CapabilitiesParser.ParseCapabilities(input);

            displayCapability.Should().BeEmpty();
        }

        [Fact]
        public void ParseCapabilities_WithBasicInput()
        {
            const string input = "(A(1)B(2))";


            var displayCapability = CapabilitiesParser.ParseCapabilities(input);

            displayCapability.Count.Should().Be(2);

            displayCapability[0].Name.Should().Be("A");
            displayCapability[0].Tags.Count.Should().Be(1);
            displayCapability[0].Tags[0].Name.Should().Be("1");

            displayCapability[1].Name.Should().Be("B");
            displayCapability[1].Tags.Count.Should().Be(1);
            displayCapability[1].Tags[0].Name.Should().Be("2");
        }

        [Fact]
        public void ParseCapabilities_WithExtendedInput()
        {
            const string input = "(A(1)B(2 3(4(5 6(7)))))";


            var displayCapability = CapabilitiesParser.ParseCapabilities(input);

            displayCapability.Count.Should().Be(2);

            displayCapability[0].Name.Should().Be("A");
            displayCapability[0].Tags.Count.Should().Be(1);
            displayCapability[0].Tags[0].Name.Should().Be("1");

            displayCapability[1].Name.Should().Be("B");
            displayCapability[1].Tags.Count.Should().Be(2);
            displayCapability[1].Tags[0].Name.Should().Be("2");
            displayCapability[1].Tags[1].Name.Should().Be("3");
            displayCapability[1].Tags[1].Tags.Count.Should().Be(1);

            displayCapability[1].Tags[1].Tags[0].Name.Should().Be("4");
            displayCapability[1].Tags[1].Tags[0].Tags.Count.Should().Be(2);

            displayCapability[1].Tags[1].Tags[0].Tags[0].Name.Should().Be("5");
            displayCapability[1].Tags[1].Tags[0].Tags[0].Tags.Count.Should().Be(0);

            displayCapability[1].Tags[1].Tags[0].Tags[1].Name.Should().Be("6");
            displayCapability[1].Tags[1].Tags[0].Tags[1].Tags.Count.Should().Be(1);

            displayCapability[1].Tags[1].Tags[0].Tags[1].Tags[0].Name.Should().Be("7");
            displayCapability[1].Tags[1].Tags[0].Tags[1].Tags[0].Tags.Count
                .Should().Be(0);
        }

        [Theory]
        [InlineData("(B(2  3))")]
        [InlineData("(B(2\n\r\t3))")]
        public void ParseCapabilities_HandlesMultipleWhitespaces(string input)
        {
            var displayCapability = CapabilitiesParser.ParseCapabilities(input);

            displayCapability.Count.Should().Be(1);

            displayCapability[0].Name.Should().Be("B");
            displayCapability[0].Tags.Count.Should().Be(2);
            displayCapability[0].Tags[0].Name.Should().Be("2");
            displayCapability[0].Tags[1].Name.Should().Be("3");
        }

        [Fact]
        public void Handles_Whitespace_After_Closing_Bracket()
        {
            const string input = "(A(1(1.1) 2))";

            var displayCapability = CapabilitiesParser.ParseCapabilities(input);

            displayCapability.Count.Should().Be(1);
            displayCapability.First().Tags.Count.Should().Be(2);
            displayCapability.First().Tags[0].Name.Should().Be("1");
            displayCapability.First().Tags[0].Tags[0].Name.Should().Be("1.1");
            displayCapability.First().Tags[1].Name.Should().Be("2");
        }

        [Fact]
        public void Handles_Whitespace_Before_Closing_Bracket()
        {
            const string input = "(A(1 2 ))";

            var displayCapability = CapabilitiesParser.ParseCapabilities(input);

            displayCapability.Count.Should().Be(1);
            displayCapability.First().Tags.Count.Should().Be(2);
            displayCapability.First().Tags[0].Name.Should().Be("1");
            displayCapability.First().Tags[1].Name.Should().Be("2");
        }
    }
}
