using Difficalcy.Osu.Models;
using Difficalcy.Osu.Services;
using Difficalcy.Services;
using Difficalcy.Tests;

namespace Difficalcy.Osu.Tests;

public class OsuCalculatorServiceTest : CalculatorServiceTest<OsuScore, OsuDifficulty, OsuPerformance, OsuCalculation>
{
    protected override CalculatorService<OsuScore, OsuDifficulty, OsuPerformance, OsuCalculation> CalculatorService { get; } = new OsuCalculatorService(new InMemoryCache(), new TestBeatmapProvider(typeof(OsuCalculatorService).Assembly.GetName().Name));

    [Theory]
    [InlineData(6.710442985146793d, 288.27290484349686d, "diffcalc-test", 0)]
    [InlineData(8.9742952703071666d, 710.7304138915342d, "diffcalc-test", 64)]
    public void Test(double expectedDifficultyTotal, double expectedPerformanceTotal, string beatmapId, int mods)
        => TestGetCalculationReturnsCorrectValues(expectedDifficultyTotal, expectedPerformanceTotal, new OsuScore { BeatmapId = beatmapId, Mods = mods });

    [Fact]
    public void TestAllParameters()
    {
        var score = new OsuScore
        {
            BeatmapId = "diffcalc-test",
            Mods = 1112, // HD, HR, DT, FL
            Combo = 200,
            Misses = 5,
            Mehs = 4,
            Oks = 3,
        };
        TestGetCalculationReturnsCorrectValues(10.07270907570737d, 553.1423675531603d, score);
    }
}
