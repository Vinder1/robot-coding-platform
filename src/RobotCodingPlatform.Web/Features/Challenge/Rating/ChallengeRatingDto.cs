namespace RobotCodingPlatform.Web.Features.Challenge.Rating;

public record ChallengeRatingEntryDto(
    int Place,
    Guid UserId,
    string UserName,
    int Iterations,
    DateTime? CompletedAt);

public record ChallengeRatingDto(
    Guid ChallengeId,
    string ChallengeName,
    int TotalParticipants,
    IReadOnlyList<ChallengeRatingEntryDto> Entries);
