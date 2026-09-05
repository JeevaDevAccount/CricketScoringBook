using Application.Abstractions.Interfaces;
using Application.Matches.Commands.MatchCommands;
using Domain.Aggregates.MatchAggregate;

namespace Application.Matches.Commands.MatchCommandHandlers;

public sealed class ClaimScorerHandler
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;

    public ClaimScorerHandler(IMatchRepository matchRepository, IUnitOfWork unitOfWork, IMatchUpdateNotifier matchUpdateNotifier){
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _matchUpdateNotifier = matchUpdateNotifier;
    }

    public async Task Handle(ClaimScorerCommand command, CancellationToken cancellationToken){
        var match = await _matchRepository.GetByIdAsync(command.MatchId,cancellationToken);
        
        if (match is null)
            throw new InvalidOperationException("Match not found.");
        
        match.ClaimScorer(command.ScorerId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _matchUpdateNotifier.NotifyMatchUpdatedAsync(command.MatchId,cancellationToken);
    }
}

public sealed class ChangeScorerHandler
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;

    public ChangeScorerHandler(IMatchRepository matchRepository, IUnitOfWork unitOfWork, IMatchUpdateNotifier matchUpdateNotifier){
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _matchUpdateNotifier = matchUpdateNotifier;
    }

    public async Task Handle(ChangeScorerCommand command, CancellationToken cancellationToken){
        var match = await _matchRepository.GetByIdAsync(command.MatchId,cancellationToken);
        
        if (match is null)
            throw new InvalidOperationException("Match not found.");
        
        match.ChangeScorer(command.ScorerId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _matchUpdateNotifier.NotifyMatchUpdatedAsync(command.MatchId,cancellationToken);
    }
}

public sealed class RecordTossHandler
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;

    public RecordTossHandler(IMatchRepository matchRepository, IUnitOfWork unitOfWork, IMatchUpdateNotifier matchUpdateNotifier){
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _matchUpdateNotifier = matchUpdateNotifier;
    }

    public async Task Handle(RecordTossCommand command, CancellationToken cancellationToken){

        var match = await _matchRepository.GetByIdAsync(command.MatchId,cancellationToken);

        if (match is null)
            throw new InvalidOperationException("Match not found.");

        match.Toss(command.TossWonTeamId, command.Decision);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _matchUpdateNotifier.NotifyMatchUpdatedAsync(command.MatchId,cancellationToken);
    }
}

public sealed class StartMatchHandler
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;

    public StartMatchHandler(IMatchRepository matchRepository, IUnitOfWork unitOfWork, IMatchUpdateNotifier matchUpdateNotifier){
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _matchUpdateNotifier = matchUpdateNotifier;
    }

    public async Task Handle(StartMatchCommand command, CancellationToken cancellationToken){
        var match = await _matchRepository.GetByIdAsync(command.MatchId,cancellationToken);
        
        if (match is null)
            throw new InvalidOperationException("Match not found.");
        
        match.StartMatch();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _matchUpdateNotifier.NotifyMatchUpdatedAsync(command.MatchId,cancellationToken);
    }
}

public sealed class StartInningsHandler
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;

    public StartInningsHandler(IMatchRepository matchRepository, IUnitOfWork unitOfWork, IMatchUpdateNotifier matchUpdateNotifier){
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _matchUpdateNotifier = matchUpdateNotifier;
    }

    public async Task Handle(StartInningsCommand command, CancellationToken cancellationToken){
        var match = await _matchRepository.GetByIdAsync(command.MatchId,cancellationToken);
        
        if (match is null)
            throw new InvalidOperationException("Match not found.");
        
        match.StartInnings(command.StrikerId,command.NonStrikerId,command.BowlerId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _matchUpdateNotifier.NotifyMatchUpdatedAsync(command.MatchId,cancellationToken);
    }
}

public sealed class RecordDeliveryHandler
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;

    public RecordDeliveryHandler(IMatchRepository matchRepository, IUnitOfWork unitOfWork, IMatchUpdateNotifier matchUpdateNotifier){
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _matchUpdateNotifier = matchUpdateNotifier;
    }

    public async Task Handle(RecordDeliveryCommand command, CancellationToken cancellationToken){
        var match = await _matchRepository.GetByIdAsync(command.MatchId,cancellationToken);
        
        if (match is null)
            throw new InvalidOperationException("Match not found.");
        
        var input = new Match.DeliveryInput(
        command.StrikerId,
        command.NonStrikerId,
        command.BowlerId,
        command.BatterRuns,
        command.TotalRuns,
        command.ExtraType,
        command.Dismissal,
        command.DismissedPlayerId);

        match.RecordDelivery(input);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _matchUpdateNotifier.NotifyMatchUpdatedAsync(command.MatchId,cancellationToken);
    }
}
public sealed class UndoDeliveryHandler
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;

    public UndoDeliveryHandler(IMatchRepository matchRepository, IUnitOfWork unitOfWork, IMatchUpdateNotifier matchUpdateNotifier){
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _matchUpdateNotifier = matchUpdateNotifier;
    }

    public async Task Handle(UndoDeliveryCommand command, CancellationToken cancellationToken){
        var match = await _matchRepository.GetByIdAsync(command.MatchId,cancellationToken);
        
        if (match is null)
            throw new InvalidOperationException("Match not found.");
        
        match.UndoDelivery();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _matchUpdateNotifier.NotifyMatchUpdatedAsync(command.MatchId, cancellationToken);
    }
}

public sealed class AddPlayerToPlayingTeamHandler{

    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;

    public AddPlayerToPlayingTeamHandler(IMatchRepository matchRepository, IUnitOfWork unitOfWork, IMatchUpdateNotifier matchUpdateNotifier){
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _matchUpdateNotifier = matchUpdateNotifier;
    }
    public async Task Handle(AddPlayerToPlayingTeamCommand command, CancellationToken cancellationToken){
        var match = await _matchRepository.GetByIdAsync(command.MatchId,cancellationToken);
        
        if (match is null)
            throw new InvalidOperationException("Match not found.");
        
        match.AddPlayerToPlayingTeam(command.TeamId,command.PlayerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _matchUpdateNotifier.NotifyMatchUpdatedAsync(command.MatchId, cancellationToken);
    }
}
