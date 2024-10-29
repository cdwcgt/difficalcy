using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Difficalcy.Models;
using Microsoft.Extensions.Logging;

namespace Difficalcy.Services
{
    public abstract class CalculatorService<TScore, TDifficulty, TPerformance, TCalculation>(ICache cache, ILogger logger)
        where TScore : Score
        where TDifficulty : Difficulty
        where TPerformance : Performance
        where TCalculation : Calculation<TDifficulty, TPerformance>
    {
        /// <summary>
        /// A set of information describing the calculator.
        /// </summary>
        public abstract CalculatorInfo Info { get; }

        /// <summary>
        /// A unique discriminator for this calculator.
        /// Should be unique for calculator that might return differing results.
        /// </summary>
        public string CalculatorDiscriminator => $"{Info.CalculatorPackage}:{Info.CalculatorVersion}";

        /// <summary>
        /// Ensures the beatmap with the given ID is available locally.
        /// </summary>
        protected abstract Task EnsureBeatmap(string beatmapId);

        /// <summary>
        /// Runs the difficulty calculator and returns the difficulty attributes as both an object and JSON serialised string.
        /// </summary>
        protected abstract (object, string) CalculateDifficultyAttributes(string beatmapId, int mods);

        /// <summary>
        /// Returns the deserialised object for a given JSON serialised difficulty attributes object.
        /// </summary>
        protected abstract object DeserialiseDifficultyAttributes(string difficultyAttributesJson);

        /// <summary>
        /// Runs the performance calculator on a given score with pre-calculated difficulty attributes and returns the performance.
        /// </summary>
        protected abstract TCalculation CalculatePerformance(TScore score, object difficultyAttributes);

        /// <summary>
        /// Returns the calculation of a given score.
        /// </summary>
        public async Task<TCalculation> GetCalculation(TScore score)
        {
            logger.LogTrace($"calculating score for {score}, bid {score.BeatmapId}, mods {score.Mods}");
            var difficultyAttributes = await GetDifficultyAttributes(score.BeatmapId, score.Mods);
            return CalculatePerformance(score, difficultyAttributes);
        }

        public async Task<IEnumerable<TCalculation>> GetCalculationBatch(TScore[] scores)
        {
            var scoresWithIndex = scores.Select((score, index) => (score, index));
            var uniqueBeatmapGroups = scoresWithIndex.GroupBy(scoreWithIndex => (scoreWithIndex.score.BeatmapId, scoreWithIndex.score.Mods));

            var calculationGroups = await Task.WhenAll(uniqueBeatmapGroups.Select(async group =>
            {
                var scores = group.Select(scoreWithIndex => scoreWithIndex.score);
                return group.Select(scoreWithIndex => scoreWithIndex.index).Zip(await GetUniqueBeatmapCalculationBatch(group.Key.BeatmapId, group.Key.Mods, scores));
            }));

            return calculationGroups.SelectMany(group => group).OrderBy(group => group.First).Select(group => group.Second);
        }

        private async Task<IEnumerable<TCalculation>> GetUniqueBeatmapCalculationBatch(string beatmapId, int mods, IEnumerable<TScore> scores)
        {
            logger.LogTrace($"calculating batch scores, bid {beatmapId}, mods {mods}");
            var difficultyAttributes = await GetDifficultyAttributes(beatmapId, mods);
            return scores.AsParallel().AsOrdered().Select(score => CalculatePerformance(score, difficultyAttributes));
        }

        private async Task<object> GetDifficultyAttributes(string beatmapId, int mods)
        {
            logger.LogTrace($"calculating beatmap difficulty, bid {beatmapId}, mods {mods}");
            await EnsureBeatmap(beatmapId);

            var db = cache.GetDatabase();
            var redisKey = $"difficalcy:{CalculatorDiscriminator}:{beatmapId}:{mods}";
            var difficultyAttributesJson = await db.GetAsync(redisKey);

            object difficultyAttributes;
            if (difficultyAttributesJson == null)
            {
                (difficultyAttributes, difficultyAttributesJson) = CalculateDifficultyAttributes(beatmapId, mods);
                db.Set(redisKey, difficultyAttributesJson);
            }
            else
            {
                difficultyAttributes = DeserialiseDifficultyAttributes(difficultyAttributesJson);
            }

            return difficultyAttributes;
        }
    }
}
