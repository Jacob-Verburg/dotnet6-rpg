using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService;

public class FightService : IFightService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public FightService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
    {
        var response = new ServiceResponse<AttackResultDto>();
        try
        {
            var attacker = await _context.Characters
                .Include(c => c.Weapon)
                .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
            var opponent = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

            var damage = DoWeaponAttack(attacker, opponent);

            if (opponent.HitPoints <= 0)
            {
                response.Message = $"{opponent.Name} has been defeated!";
            }

            await _context.SaveChangesAsync();

            response.Data = new AttackResultDto
            {
                Attacker = attacker.Name,
                Opponent = opponent.Name,
                AttackerHp = attacker.HitPoints,
                OpponentHp = opponent.HitPoints,
                Damage = damage
            };
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Message = e.Message;
        }

        return response;
    }

    private static int DoWeaponAttack(Character? attacker, Character? opponent)
    {
        int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
        damage -= new Random().Next(opponent.Defense);

        if (damage > 0)
        {
            opponent.HitPoints -= damage;
        }

        return damage;
    }

    public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
    {
        var response = new ServiceResponse<AttackResultDto>();
        try
        {
            var attacker = await _context.Characters
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
            var opponent = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

            var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);

            if (skill == null)
            {
                response.Success = false;
                response.Message = $"{attacker.Name} doesn't have that skill.";
                return response;
            }
            
            var damage = DoSkillAttack(skill, attacker, opponent);

            if (opponent.HitPoints <= 0)
            {
                response.Message = $"{opponent.Name} has been defeated!";
            }

            await _context.SaveChangesAsync();

            response.Data = new AttackResultDto
            {
                Attacker = attacker.Name,
                Opponent = opponent.Name,
                AttackerHp = attacker.HitPoints,
                OpponentHp = opponent.HitPoints,
                Damage = damage
            };
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Message = e.Message;
        }

        return response;
    }

    private static int DoSkillAttack(Skill skill, Character attacker, Character? opponent)
    {
        int damage = skill.Damage + (new Random().Next(attacker.Intelligence));
        damage -= new Random().Next(opponent.Defense);

        if (damage > 0)
        {
            opponent.HitPoints -= damage;
        }

        return damage;
    }

    public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
    {
        var response = new ServiceResponse<FightResultDto>
        {
            Data = new FightResultDto()
        };
        
        try
        {
            var characters = await _context.Characters
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .Where(c => request.CharacterIds.Contains(c.Id)).ToListAsync();

            bool defeated = false;
            while (!defeated)
            {
                foreach (Character attacker in characters)
                {
                    var opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                    var opponent = opponents[new Random().Next(opponents.Count)];

                    int damage = 0;
                    string attackUsed = string.Empty;

                    bool useWeapon = new Random().Next(2) == 0;
                    if (useWeapon)
                    {
                        attackUsed = attacker.Weapon.Name;
                        damage = DoWeaponAttack(attacker, opponent);
                    }
                    else
                    {
                        var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];
                        attackUsed = skill.Name;
                        damage = DoSkillAttack(skill, attacker, opponent);
                    }
                    response.Data.Log
                        .Add($"{attacker.Name} attacks {opponent.Name} using {attackUsed} for {(damage >= 0 ? damage:0)} damage.");

                    if (opponent.HitPoints <= 0)
                    {
                        defeated = true;
                        attacker.Victories++;
                        opponent.Defeats++;
                        response.Data.Log.Add($"{opponent.Name} has been defeaed!");
                        response.Data.Log.Add($"{attacker.Name} wins with {attacker.HitPoints} HP left!");
                        break;
                    }
                }
            }
            foreach (var character in characters)
            {
                character.Fights++;
                character.HitPoints = 100;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Message = e.Message;
        }

        return response;


    }

    public async Task<ServiceResponse<List<HighscoreDto>>> GetHighscore()
    {
        var characters = await _context.Characters
            .Where(c => c.Fights > 0)
            .OrderByDescending(c =>c.Victories)
            .ThenBy(c => c.Defeats)
            .ToListAsync();

        var response = new ServiceResponse<List<HighscoreDto>>
        {
            Data = characters.Select(c => _mapper.Map<HighscoreDto>(c)).ToList()
        };

        return response;
    }
}