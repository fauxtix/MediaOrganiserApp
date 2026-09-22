using MediaOrganizerApp.Application.DTOs.AI;
using Xunit;

namespace MediaOrganizerApp.Tests;

public class ModeloUsageProviderTests
{
    [Theory]
    [InlineData("openrouter/free", "OpenRouter")]
    [InlineData("nex-agi/nex-n2.5-pro:free", "OpenRouter")]
    [InlineData("meta-llama/llama-3.3-70b-instruct:free", "OpenRouter")]
    [InlineData("phi4-mini:latest", "Ollama")]
    [InlineData("llama3.2", "Ollama")]
    [InlineData("qwen2.5:7b", "Ollama")]
    [InlineData("", "Ollama")]
    [InlineData(null, "Ollama")]
    public void ProviderFromModelName_Classifies_Correctly(string? model, string expected)
    {
        Assert.Equal(expected, ModeloUsageContagem.ProviderFromModelName(model!));
    }
}