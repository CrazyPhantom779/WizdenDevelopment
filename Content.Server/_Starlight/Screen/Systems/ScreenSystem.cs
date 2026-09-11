<<<<<<< HEAD:Content.Server/Screens/Systems/ScreenSystem.cs
using Content.Shared.TextScreen;
using Content.Server.Screens.Components;
using Content.Shared.DeviceNetwork.Components;
=======
using Content.Shared.Screen.Components;
using Content.Server.AlertLevel;
>>>>>>> Starlight/starlight-dev:Content.Server/_Starlight/Screen/Systems/ScreenSystem.cs
using Content.Shared.DeviceNetwork.Events;
using Robust.Shared.Timing;
using Content.Shared.TextScreen;
using Content.Shared.Station.Components; // Starlight

<<<<<<< HEAD:Content.Server/Screens/Systems/ScreenSystem.cs
namespace Content.Server.Screens.Systems;
=======

namespace Content.Server._Starlight.Screen.Systems;
>>>>>>> Starlight/starlight-dev:Content.Server/_Starlight/Screen/Systems/ScreenSystem.cs

/// <summary>
/// Controls the wallmounted screens on stations and shuttles displaying e.g. FTL duration, ETA
/// </summary>
public sealed partial class ScreenSystem : EntitySystem
{
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private SharedAppearanceSystem _appearanceSystem = default!;
<<<<<<< HEAD:Content.Server/Screens/Systems/ScreenSystem.cs
=======

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScreenComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertLevelChanged);
    }
>>>>>>> Starlight/starlight-dev:Content.Server/_Starlight/Screen/Systems/ScreenSystem.cs

    /// <summary>
    /// Calls either a normal screen text update or shuttle timer update based on the presence of
    /// <see cref="ShuttleTimerMasks.ShuttleMap"/> in <see cref="args.Data"/>
    /// </summary>
    [SubscribeLocalEvent]
    private void OnPacketReceived(Entity<ScreenComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        if (args.Data.TryGetValue(ShuttleTimerMasks.ShuttleMap, out _))
            ShuttleTimer(ent, args);
        else
            ScreenText(ent, args);
    }

    private void OnAlertLevelChanged(AlertLevelChangedEvent args)
    {
        var query = EntityQueryEnumerator<ScreenComponent>();
        while (query.MoveNext(out var uid, out var screen))
        {
            //Starlight begin
            if (!TryComp<StationMemberComponent>(Transform(uid).GridUid, out var stationMember)) continue;
            if (stationMember.Station != args.Station) continue;
            //Starlight end
            _appearanceSystem.SetData(uid, TextScreenVisuals.AlertLevel, args.AlertLevel);
        }
    }

    /// <summary>
    /// Send a text update to every screen on the same MapUid as the originating comms console.
    /// </summary>
    private void ScreenText(Entity<ScreenComponent> ent, DeviceNetworkPacketEvent args)
    {
        // don't allow text updates if there's an active timer
<<<<<<< HEAD:Content.Server/Screens/Systems/ScreenSystem.cs
        // (and just check here so the server doesn't have to track them)
        if (_appearanceSystem.TryGetData(ent, TextScreenVisuals.TargetTime, out TimeSpan target)
=======
        if (HasComp<ScreenTimerComponent>(uid)
            && _appearanceSystem.TryGetData(uid, TextScreenVisuals.TargetTime, out TimeSpan target)
>>>>>>> Starlight/starlight-dev:Content.Server/_Starlight/Screen/Systems/ScreenSystem.cs
            && target > _gameTiming.CurTime)
            return;

        var screenMap = Transform(ent).MapUid;
        var argsMap = Transform(args.Sender).MapUid;

        if (screenMap == null || argsMap == null || screenMap != argsMap)
        {
<<<<<<< HEAD:Content.Server/Screens/Systems/ScreenSystem.cs
            _appearanceSystem.SetData(ent, TextScreenVisuals.DefaultText, text);
            _appearanceSystem.SetData(ent, TextScreenVisuals.ScreenText, text);
            _appearanceSystem.SetData(ent, TextScreenVisuals.ScreenTextTime, _gameTiming.CurTime);
=======
            Log.Error($"Can't update screen text, while sender or screen on another map!");
            return;
>>>>>>> Starlight/starlight-dev:Content.Server/_Starlight/Screen/Systems/ScreenSystem.cs
        }

        if (!args.Data.TryGetValue(ScreenMasks.Text, out string? text) || text == null)
        {
            Log.Error($"Can't update screen text, while input text was null!");
            return;
        }

        _appearanceSystem.SetData(uid, TextScreenVisuals.DefaultText, text);
        _appearanceSystem.SetData(uid, TextScreenVisuals.ScreenText, text);
    }

    /// <summary>
    /// Determines if/how a timer packet affects this screen.
    /// Currently there are 2 broadcast domains: Arrivals, and every other screen.
    /// Domain is determined by the <see cref="DeviceNetworkComponent.TransmitFrequencyId"/> on each timer.
    /// Each broadcast domain is divided into subnets. Screen MapUid determines subnet.
    /// Subnets are the shuttle, source, and dest. Source/dest change each jump.
    /// This is required to send different timers to the shuttle/terminal/station.
    /// </summary>
    private void ShuttleTimer(Entity<ScreenComponent> ent, DeviceNetworkPacketEvent args)
    {
        var timerXform = Transform(ent);

        // no false positives.
        if (timerXform.MapUid == null)
            return;

        string key;
        args.Data.TryGetValue(ShuttleTimerMasks.ShuttleMap, out EntityUid? shuttleMap);
        args.Data.TryGetValue(ShuttleTimerMasks.SourceMap, out EntityUid? source);
        args.Data.TryGetValue(ShuttleTimerMasks.DestMap, out EntityUid? dest);
        args.Data.TryGetValue(ShuttleTimerMasks.Docked, out bool docked);
        string text = docked ? ShuttleTimerMasks.ETD : ShuttleTimerMasks.ETA;

        switch (timerXform.MapUid)
        {
            // sometimes the timer transforms on FTL shuttles have a hyperspace mapuid, so matching by grid works as a fallback.
            case var local when local == shuttleMap || timerXform.GridUid == shuttleMap:
                key = ShuttleTimerMasks.ShuttleTime;
                break;
            case var origin when origin == source:
                key = ShuttleTimerMasks.SourceTime;
                break;
            case var remote when remote == dest:
                key = ShuttleTimerMasks.DestTime;
                text = ShuttleTimerMasks.ETA;
                break;
            default:
                return;
        }

        if (!args.Data.TryGetValue(key, out TimeSpan duration))
            return;

        if (args.Data.TryGetValue(ScreenMasks.Text, out string? label) && label != null)
            text = label;

        _appearanceSystem.SetData(ent, TextScreenVisuals.ScreenText, text);
        _appearanceSystem.SetData(ent, TextScreenVisuals.TargetTime, _gameTiming.CurTime + duration);
        _appearanceSystem.SetData(ent, TextScreenVisuals.ScreenTextTime, _gameTiming.CurTime);

        if (args.Data.TryGetValue(ScreenMasks.Color, out Color color))
            _appearanceSystem.SetData(ent, TextScreenVisuals.Color, color);
    }
}
