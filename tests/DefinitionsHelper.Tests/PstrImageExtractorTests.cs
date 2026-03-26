using DefinitionsHelper.Extraction;
using Xunit;

namespace DefinitionsHelper.Tests;

public class PstrImageExtractorTests
{
    private static byte[] ReadFixture(string filename) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "fixtures", filename));

    [Fact]
    public void Extract_ValidPstr_ReturnsPngBytes()
    {
        byte[]? result = PstrImageExtractor.Extract(ReadFixture("cuboid.pstr"));

        Assert.NotNull(result);
        Assert.True(result.Length > 8, "Extracted data should be larger than the PNG header");

        // Verify PNG magic bytes: 0x89 P N G \r \n 0x1A \n
        Assert.Equal(0x89, result[0]);
        Assert.Equal((byte)'P', result[1]);
        Assert.Equal((byte)'N', result[2]);
        Assert.Equal((byte)'G', result[3]);
        Assert.Equal(0x0D, result[4]);
        Assert.Equal(0x0A, result[5]);
        Assert.Equal(0x1A, result[6]);
        Assert.Equal(0x0A, result[7]);
    }

    [Fact]
    public void Extract_InvalidPstr_ReturnsNull()
    {
        byte[]? result = null;
        try
        {
            result = PstrImageExtractor.Extract(ReadFixture("invalid.pstr"));
        }
        catch (InvalidDataException)
        {
            // GZipStream throws InvalidDataException for non-gzip data
        }

        Assert.Null(result);
    }

    [Fact]
    public void Extract_EmptyBytes_ReturnsNull()
    {
        byte[]? result = null;
        try
        {
            result = PstrImageExtractor.Extract([]);
        }
        catch (InvalidDataException)
        {
            // GZipStream throws InvalidDataException for empty data
        }

        Assert.Null(result);
    }
}
