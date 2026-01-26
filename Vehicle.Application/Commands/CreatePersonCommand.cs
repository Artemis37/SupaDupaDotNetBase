using Microsoft.Extensions.Options;
using Shared.Application.Context;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using System.ComponentModel.DataAnnotations;
using Vehicle.Application.Decorators;
using Vehicle.Application.Models;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Models;
using ValidationAttribute = Vehicle.Application.Decorators.ValidationAttribute;

namespace Vehicle.Application.Commands;

public class CreatePersonCommand : ICommand
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string Password { get; set; } = string.Empty;
}

[LoggingCommand]
[Validation]
public class CreatePersonCommandHandler : ICommandHandler<CreatePersonCommand>
{
    private readonly IPersonMasterRepository _personMasterRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IOptions<ShardingSettings> _shardingSettings;

    public CreatePersonCommandHandler(
        IPersonMasterRepository personMasterRepository,
        IPersonRepository personRepository,
        IOptions<ShardingSettings> shardingSettings)
    {
        _personMasterRepository = personMasterRepository;
        _personRepository = personRepository;
        _shardingSettings = shardingSettings;
    }

    public async Task<Result> Handle(CreatePersonCommand command)
    {
        var existingUser = await _personMasterRepository.GetByUsernameAsync(command.Username);
        if (existingUser != null)
        {
            return Result.Fail("Username is already taken");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.Password);

        int shardId;
        if (_shardingSettings.Value.HotShard.HasValue && 
            _shardingSettings.Value.HotShard.Value >= 1 && 
            _shardingSettings.Value.HotShard.Value <= _shardingSettings.Value.TotalShards)
        {
            shardId = _shardingSettings.Value.HotShard.Value;
        }
        else
        {
            shardId = Random.Shared.Next(1, _shardingSettings.Value.TotalShards + 1);
        }

        // TODO: Replace direct database call with message broker for eventual consistency
        // Potential optimizations:
        // 1. SAGA Pattern: Implement compensating transactions to rollback Master DB 
        //    if Sharding DB creation fails (e.g., delete PersonMaster if Person creation fails)
        // 2. Outbox Pattern with Idempotency: Store "PersonCreated" event in Master DB outbox table,
        //    process via background worker to create Person in Sharding DB, ensuring at-least-once
        //    delivery with idempotent handlers to prevent duplicate Person records
        // 3. Event Sourcing: Store registration as immutable event, rebuild state by replaying events
        // 4. Two-Phase Message: Publish "PersonCreating" (prepare), then "PersonCreated" (commit) 
        //    or "PersonCreationFailed" (abort) events to message broker
        // 5. Async Creation: Return success immediately after Master DB write, create Sharding DB 
        //    record asynchronously via message queue (eventual consistency trade-off)
        
        PersonMaster? personMaster = null;
        
        try
        {
            var personSyncId = Guid.CreateVersion7();

            personMaster = new PersonMaster
            {
                Username = command.Username,
                Password = hashedPassword,
                PersonSyncId = personSyncId,
                ShardId = shardId,
                CreatedAt = DateTime.UtcNow
            };

            personMaster = await _personMasterRepository.AddAsync(personMaster);
            await _personMasterRepository.SaveChangesAsync();

            PersonContextProvider.SetCurrent(new PersonContext
            {
                PersonId = personMaster.Id,
                ShardId = personMaster.ShardId
            });

            _personRepository.ReloadShardingDbContext();

            var person = new Person
            {
                Name = command.Username,
                PersonSyncId = personSyncId,
                CreatedBy = personMaster.Id,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _personRepository.AddAsync(person);
            await _personRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            if (personMaster != null && personMaster.Id > 0)
            {
                try
                {
                    await _personMasterRepository.DeleteAsync(personMaster);
                    await _personMasterRepository.SaveChangesAsync();
                }
                catch
                {}
            }
            throw;
        }

        return Result.Ok();
    }
}
