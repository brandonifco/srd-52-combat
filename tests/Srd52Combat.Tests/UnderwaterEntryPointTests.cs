using Srd52Combat.Attacks;
using Srd52Combat.Requests;
using Srd52Combat.Underwater;
using Xunit;
using static Srd52Combat.Tests.AttackFixtures;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// Underwater combat, through the entry points only: the impeded weapons of "Combat / Impeded
/// Weapons / p. 16" (<c>underwater-melee</c>, <c>underwater-ranged</c>) and the Fire Resistance of
/// "Combat / Fire Resistance / p. 16" (<c>underwater-fire-resistance</c>).
/// </summary>
public class UnderwaterEntryPointTests
{
    private static readonly UnderwaterStatement Underwater = UnderwaterStatement.Is(Gm);

    private static readonly UnderwaterStatement OnDryLand = UnderwaterStatement.IsNot(Gm);

    [Fact]
    public void Underwater_a_melee_weapon_attack_has_Disadvantage_only_without_a_Swim_Speed_and_without_Piercing_citing_page_16()
    {
        // All four combinations of Swim Speed and Piercing.
        (bool SwimSpeed, bool Piercing, RollEffect Effect)[] cases =
        [
            (false, false, RollEffect.Disadvantage),
            (false, true, RollEffect.None),
            (true, false, RollEffect.None),
            (true, true, RollEffect.None),
        ];

        foreach (var (swimSpeed, piercing, effect) in cases)
        {
            var determination = Value<RollDetermination>(EntryPoints.UnderwaterMelee.Resolve(new UnderwaterMeleeRequest
            {
                Where = Underwater,
                SwimSpeed = swimSpeed ? SwimSpeedStatement.Has(Gm) : SwimSpeedStatement.Lacks(Gm),
                Weapon = piercing
                    ? WeaponStatement.Piercing("Spear", Gm)
                    : WeaponStatement.NotPiercing("Warhammer", Gm),
            }));

            Assert.Equal(effect, determination.Effect);
            Assert.Equal("underwater-melee", determination.EntryId);
            Assert.Equal("Combat / Impeded Weapons / p. 16", determination.Authority.Citation);
            Assert.Equal(EntryPoints.UnderwaterMelee.Registered.Locator, determination.Authority);
        }
    }

    [Fact]
    public void Out_of_the_water_the_melee_rule_gives_the_attack_roll_nothing()
    {
        var determination = Value<RollDetermination>(EntryPoints.UnderwaterMelee.Resolve(new UnderwaterMeleeRequest
        {
            Where = OnDryLand,
            SwimSpeed = SwimSpeedStatement.Lacks(Gm),
            Weapon = WeaponStatement.NotPiercing("Warhammer", Gm),
        }));

        Assert.Equal(RollEffect.None, determination.Effect);
        Assert.Contains("not underwater", determination.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void Underwater_a_ranged_weapon_attack_misses_beyond_normal_range_and_has_Disadvantage_within_it_citing_page_16()
    {
        (int Distance, bool Misses, RollEffect Effect)[] cases =
        [
            (100, false, RollEffect.Disadvantage),
            (Longbow.NormalFeet, false, RollEffect.Disadvantage),
            (Longbow.NormalFeet + 1, true, RollEffect.None),
            (Longbow.LongFeet + 1, true, RollEffect.None),
        ];

        foreach (var (distance, misses, effect) in cases)
        {
            var attack = Value<UnderwaterRangedAttack>(EntryPoints.UnderwaterRanged.Resolve(new UnderwaterRangedRequest
            {
                Where = Underwater,
                Ranges = Longbow,
                DistanceFeet = distance,
            }));

            Assert.Equal(misses, attack.AutomaticallyMisses);
            Assert.Equal(effect, attack.Effect);
            Assert.Equal("Combat / Impeded Weapons / p. 16", attack.Authority.Citation);
            Assert.Equal(EntryPoints.UnderwaterRanged.Registered.Locator, attack.Authority);
            Assert.Equal("Combat / Range / p. 15", attack.Range.Authority.Citation);
        }

        // Beyond long range, normal-and-long-range also says the attack can't be made at all.
        var beyondLong = Value<UnderwaterRangedAttack>(EntryPoints.UnderwaterRanged.Resolve(new UnderwaterRangedRequest
        {
            Where = Underwater,
            Ranges = Longbow,
            DistanceFeet = Longbow.LongFeet + 1,
        }));

        Assert.False(beyondLong.Range.CanAttack);
    }

    [Fact]
    public void Out_of_the_water_the_ranged_rule_gives_the_attack_roll_nothing()
    {
        var attack = Value<UnderwaterRangedAttack>(EntryPoints.UnderwaterRanged.Resolve(new UnderwaterRangedRequest
        {
            Where = OnDryLand,
            Ranges = Longbow,
            DistanceFeet = Longbow.NormalFeet + 1,
        }));

        Assert.False(attack.AutomaticallyMisses);
        Assert.Equal(RollEffect.None, attack.Effect);
        Assert.Contains("not underwater", attack.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void Anything_underwater_has_Resistance_to_Fire_damage_naming_where_Resistance_is_explained_citing_page_16()
    {
        var resistance = Value<FireResistance>(EntryPoints.UnderwaterFireResistance.Resolve(
            new UnderwaterFireResistanceRequest { Where = Underwater }));

        Assert.True(resistance.HasResistanceToFireDamage);
        Assert.Equal("Combat / Fire Resistance / p. 16", resistance.Authority.Citation);
        Assert.Equal(EntryPoints.UnderwaterFireResistance.Registered.Locator, resistance.Authority);
        Assert.Equal(EntryPoints.Resistance.Registered.Locator, resistance.ResistanceAuthority);
    }

    [Fact]
    public void Out_of_the_water_the_rule_gives_no_Resistance_to_Fire_damage()
    {
        var resistance = Value<FireResistance>(EntryPoints.UnderwaterFireResistance.Resolve(
            new UnderwaterFireResistanceRequest { Where = OnDryLand }));

        Assert.False(resistance.HasResistanceToFireDamage);
        Assert.Null(resistance.ResistanceAuthority);
    }

    [Fact]
    public void A_statement_an_underwater_rule_needs_is_refused_when_left_out_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.UnderwaterMelee.Resolve(new UnderwaterMeleeRequest
        {
            SwimSpeed = SwimSpeedStatement.Lacks(Gm),
            Weapon = WeaponStatement.NotPiercing("Warhammer", Gm),
        }));
        Assert.Throws<ArgumentException>(() => EntryPoints.UnderwaterRanged.Resolve(new UnderwaterRangedRequest
        {
            Ranges = Longbow,
            DistanceFeet = 100,
        }));
        Assert.Throws<ArgumentException>(() => EntryPoints.UnderwaterFireResistance.Resolve(
            new UnderwaterFireResistanceRequest()));
    }
}
