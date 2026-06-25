using NewsCollector.Application.Options;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Linking;

public static class TopicLinkClassifier
{
    public static TopicLinkDecision? Classify(TopicLinkSignals signals, TopicLinkerOptions options)
    {
        if (signals.TitleJaccard >= options.DuplicateSimilarity)
        {
            return new TopicLinkDecision(
                LinkType.Duplicate,
                LinkMethod.TitleSimilarity,
                signals.TitleJaccard);
        }

        if (signals.EmbeddingCosine >= options.DuplicateEmbeddingSimilarity)
        {
            return new TopicLinkDecision(
                LinkType.Duplicate,
                LinkMethod.Embedding,
                signals.EmbeddingCosine);
        }

        var titleMatch = signals.TitleJaccard >= options.MinSimilarity
            && signals.SharedTokens >= options.MinSharedTokens;

        var embeddingSameTopic = signals.EmbeddingCosine >= options.MinEmbeddingSimilarityForSameTopic;

        var entitySameTopic = signals.SharedEntityCount >= options.MinSharedEntitiesForSameTopic
            && (signals.EntityJaccard >= options.MinEntityJaccardForSameTopic
                || signals.EmbeddingCosine >= options.MinEmbeddingSimilarityWithEntities);

        if (titleMatch || embeddingSameTopic || entitySameTopic)
        {
            var confidence = Max(
                signals.TitleJaccard,
                signals.EmbeddingCosine,
                signals.EntityJaccard * 0.95m);

            return new TopicLinkDecision(
                LinkType.SameTopic,
                ResolveMethod(signals, preferEmbedding: embeddingSameTopic || entitySameTopic),
                confidence);
        }

        var embeddingRelated = signals.EmbeddingCosine >= options.MinEmbeddingSimilarityForRelated;
        var entityRelated = signals.SharedEntityCount >= options.MinSharedEntitiesForRelated
            && signals.EntityJaccard >= options.MinEntityJaccardForRelated;
        var weakTitleRelated = signals.TitleJaccard >= options.MinRelatedTitleSimilarity
            && signals.SharedTokens >= 1
            && (embeddingRelated || entityRelated);

        if (embeddingRelated || entityRelated || weakTitleRelated)
        {
            var confidence = Max(
                signals.EmbeddingCosine * 0.95m,
                signals.EntityJaccard * 0.90m,
                signals.TitleJaccard * 0.85m);

            if (confidence < options.MinRelatedTitleSimilarity)
            {
                return null;
            }

            return new TopicLinkDecision(
                LinkType.Related,
                ResolveMethod(signals, preferEmbedding: embeddingRelated),
                confidence);
        }

        return null;
    }

    private static LinkMethod ResolveMethod(TopicLinkSignals signals, bool preferEmbedding)
    {
        var methods = 0;
        if (signals.TitleJaccard > 0) methods++;
        if (signals.EmbeddingCosine > 0) methods++;
        if (signals.SharedEntityCount > 0) methods++;

        if (methods >= 2)
        {
            return LinkMethod.Hybrid;
        }

        if (preferEmbedding && signals.EmbeddingCosine > 0)
        {
            return LinkMethod.Embedding;
        }

        if (signals.SharedEntityCount > 0 && signals.TitleJaccard == 0 && signals.EmbeddingCosine == 0)
        {
            return LinkMethod.EntityOverlap;
        }

        if (signals.EmbeddingCosine > signals.TitleJaccard)
        {
            return LinkMethod.Embedding;
        }

        if (signals.SharedEntityCount > 0)
        {
            return LinkMethod.EntityOverlap;
        }

        return LinkMethod.TitleSimilarity;
    }

    private static decimal Max(params decimal[] values) =>
        values.Length == 0 ? 0m : values.Max();
}

public readonly record struct TopicLinkSignals(
    decimal TitleJaccard,
    int SharedTokens,
    decimal EmbeddingCosine,
    int SharedEntityCount,
    decimal EntityJaccard);

public readonly record struct TopicLinkDecision(
    LinkType LinkType,
    LinkMethod LinkMethod,
    decimal Confidence);
