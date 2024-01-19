using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using System.ComponentModel;

namespace SpecialRounds;
[MinimumApiVersion(120)]

public static class GetUnixTime
{
    public static int GetUnixEpoch(this DateTime dateTime)
    {
        var unixTime = dateTime.ToUniversalTime() -
                       new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return (int)unixTime.TotalSeconds;
    }
}
public partial class SpecialRounds : BasePlugin, IPluginConfig<ConfigSpecials>
{
    public override string ModuleName => "SpecialRounds";
    public override string ModuleAuthor => "DeadSwim, H3adsho0ot";
    public override string ModuleDescription => "Simple Special rounds.";
    public override string ModuleVersion => "V. 1.0.6";
    private static readonly int?[] IsVIP = new int?[65];
    public CounterStrikeSharp.API.Modules.Timers.Timer? timer_up;
    public CounterStrikeSharp.API.Modules.Timers.Timer? timer_decoy;

    public ConfigSpecials Config { get; set; }
    public int Round;
    public int IsRoundNumber;
    public string NameOfRound = "";
    public bool isset = false;

    public bool wasSpecialRound = false;
    public int currentTick = 0;
    public int showIntervalTicks = 640;

    public void OnConfigParsed(ConfigSpecials config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        WriteColor("Special round is [*Loaded*]", ConsoleColor.Green);

        RegisterListener<Listeners.OnMapStart>(name =>
        {
            NameOfRound = "";
            IsRoundNumber = 0;
            Round = 0;

        });

        RegisterListener<Listeners.OnTick>(() =>
        {
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                var ent = NativeAPI.GetEntityFromIndex(i);
                if (ent != 0)
                {
                    var client = new CCSPlayerController(ent);
                    if (client != null || client.IsValid)
                    {
                        if (currentTick < showIntervalTicks)
                        {
                            client.PrintToCenterHtml(
                            $"<font color='gray'>----</font> <font class='fontSize-l' color='green'>Special Rounds</font><font color='gray'>----</font><br>" +
                            $"<font color='gray'>Now playing</font> <font class='fontSize-m' color='green'>[{NameOfRound}]</font>"
                            );
                        }

                        OnTick(client);
                    }
                }
            }

            if (currentTick < showIntervalTicks)
            {
                currentTick = currentTick + 1;
            }
        });
    }

    public static SpecialRounds It;

    public SpecialRounds()
    {
        It = this;
    }

    public static void OnTick(CCSPlayerController controller)
    {
        if (!controller.PawnIsAlive)
        {
            return;
        }

        var buttons = controller.Buttons;

        if (It.IsRoundNumber != 6)
        {
            return;
        }
        if (buttons == PlayerButtons.Attack2)
        {
            return;
        }
        if (buttons == PlayerButtons.Zoom)
        {
            return;
        }
    }

    [ConsoleCommand("css_startround", "Start specific round")]
    public void startround(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            if (AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                int round_id = Convert.ToInt32(info.ArgByIndex(1));
                IsRoundNumber = round_id;
                player.PrintToChat("YOU START A ROUND!");
            }
        }
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        WriteColor($"SpecialRound - [*SUCCESS*] I turning off the special round.", ConsoleColor.Green);

        change_cvar("sv_autobunnyhopping", "false");
        change_cvar("sv_enablebunnyhopping", "false");
        change_cvar("sv_gravity", "800");
        change_cvar("mp_buytime", $"{Config.mp_buytime}");

        if (timer_up != null)
        {
            timer_up.Kill();
        }
        if (timer_decoy != null)
        {
            timer_decoy.Kill();
        }

        foreach (var player_l in Utilities.GetPlayers().Where(player => player is { IsValid: true }))
        {
            player_l.PlayerPawn.Value!.VelocityModifier = 0.0f;
        }

        isset = false;
        IsRoundNumber = 0;
        NameOfRound = "Normal mode";

        if (!wasSpecialRound)
        {
            Random rnd = new Random();
            int random = rnd.Next(1, 9);

            IsRoundNumber = random;

            bool foundNextGame = false;
            while (foundNextGame == false)
            {
                switch (random)
                {
                    case 1:
                        if (Config.AllowKnifeRound)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Knife only";
                        break;
                    case 2:
                        if (Config.AllowBHOPRound)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Auto BHopping";
                        break;
                    case 3:
                        if (Config.AllowGravityRound)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Gravity round";
                        break;
                    case 4:
                        if (Config.AllowAWPRound)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Only AWP";
                        break;
                    case 5:
                        if (Config.AllowP90Round)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Only P90";
                        break;
                    case 6:
                        if (Config.AllowANORound)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Only AWP + NOSCOPE";
                        break;
                    case 7:
                        if (Config.AllowSlapRound)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Slaping round";
                        break;
                    case 8:
                        if (Config.AllowDecoyRound)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Decoy round";
                        break;
                    case 9:
                        if (Config.AllowSpeedRound)
                        {
                            foundNextGame = true;
                        }
                        NameOfRound = "Speed round";
                        break;
                }

                random = random + 1;
                if (random > 9)
                {
                    random = 1;
                }
            }

            wasSpecialRound = true;
        }
        else
        {
            wasSpecialRound = false;
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);

        foreach (var l_player in Utilities.GetPlayers())
        {
            CCSPlayerController player = l_player;
            var client = player.Index;

            switch (IsRoundNumber)
            {
                case 1:
                    change_cvar("mp_buytime", "0");

                    if (is_alive(player))
                    {
                        foreach (var weapon in player.PlayerPawn.Value.WeaponServices!.MyWeapons)
                        {
                            if (weapon is { IsValid: true, Value.IsValid: true })
                            {
                                if (!weapon.Value.DesignerName.Contains("bayonet") || !weapon.Value.DesignerName.Contains("knife"))
                                {
                                    weapon.Value.Remove();
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    change_cvar("sv_autobunnyhopping", "true");
                    change_cvar("sv_enablebunnyhopping", "true");
                    break;
                case 3:
                    change_cvar("sv_gravity", "400");
                    break;
                case 4:
                    change_cvar("mp_buytime", "0");

                    if (is_alive(player))
                    {
                        foreach (var weapon in player.PlayerPawn.Value.WeaponServices!.MyWeapons)
                        {
                            if (weapon is { IsValid: true, Value.IsValid: true })
                            {
                                weapon.Value.Remove();
                            }
                        }
                        player.GiveNamedItem("weapon_awp");
                    }
                    break;
                case 5:
                    change_cvar("mp_buytime", "0");

                    if (is_alive(player))
                    {
                        foreach (var weapon in player.PlayerPawn.Value.WeaponServices!.MyWeapons)
                        {
                            if (weapon is { IsValid: true, Value.IsValid: true })
                            {
                                weapon.Value.Remove();
                            }
                        }
                        player.GiveNamedItem("weapon_p90");
                    }
                    break;
                case 6:
                    change_cvar("mp_buytime", "0");

                    if (is_alive(player))
                    {
                        foreach (var weapon in player.PlayerPawn.Value.WeaponServices!.MyWeapons)
                        {
                            if (weapon is { IsValid: true, Value.IsValid: true })
                            {
                                if (!weapon.Value.DesignerName.Contains("bayonet") || !weapon.Value.DesignerName.Contains("knife"))
                                {
                                    weapon.Value.Remove();
                                }
                            }
                        }
                        player.GiveNamedItem("weapon_awp");
                    }
                    break;
                case 7:
                    Random rnd = new Random();
                    int random = rnd.Next(3, 10);
                    float random_time = random;
                    timer_up = AddTimer(random + 0.1f, () => { goup(player); }, TimerFlags.REPEAT);
                    break;
                case 8:
                    change_cvar("mp_buytime", "0");

                    if (is_alive(player))
                    {
                        foreach (var weapon in player.PlayerPawn.Value!.WeaponServices!.MyWeapons)
                        {
                            if (weapon is { IsValid: true, Value.IsValid: true })
                            {
                                if (!weapon.Value.DesignerName.Contains("bayonet") || !weapon.Value.DesignerName.Contains("knife"))
                                {
                                    weapon.Value.Remove();
                                }
                            }
                        }
                        player.PlayerPawn.Value!.Health = 1;
                        player.GiveNamedItem("weapon_decoy");

                        timer_decoy = AddTimer(2.0f, () => { DecoyCheck(player); }, TimerFlags.REPEAT);
                    }
                    break;
                case 9:
                    if (is_alive(player))
                    {
                        CCSPlayerPawn? pawn = player.PlayerPawn.Value;
                        pawn.VelocityModifier = 2.0f;
                        player.PlayerPawn.Value!.Health = 200;
                    }
                    break;
            }
        }

        isset = false;
        currentTick = 0;

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;
        CCSPlayerController attacker = @event.Attacker;

        if (player.Connected == PlayerConnectedState.PlayerConnected || player.PlayerPawn.IsValid || @event.Userid.IsValid)
        {
            if (IsRoundNumber == 8)
            {
                if (@event.Weapon != "decoy")
                {
                    player.PlayerPawn.Value!.Health = 1;
                    player.PrintToChat($" {Config.Prefix} You canno't hit player with other GUN!");
                }
            }
            @event.Userid.PlayerPawn.Value!.VelocityModifier = 1;
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnWeaponZoom(EventWeaponZoom @event, GameEventInfo info)
    {
        if (IsRoundNumber != 6)
        {
            return HookResult.Continue;
        }

        var player = @event.Userid;
        var weaponservices = player.PlayerPawn.Value.WeaponServices!;
        var currentWeapon = weaponservices.ActiveWeapon.Value.DesignerName;

        weaponservices.ActiveWeapon.Value.Remove();
        player.GiveNamedItem(currentWeapon);

        return HookResult.Continue;
    }
}

