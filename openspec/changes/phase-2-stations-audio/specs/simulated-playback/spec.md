# Spec Delta

## REMOVED Requirements

### Requirement: Selected station on the player

**Reason**: The simulated player is replaced by the real `audio-playback` capability.

**Migration**: Use the `audio-playback` specification; the player still shows the selected station's name and country and now also shows its logo.

### Requirement: Simulated play and stop

**Reason**: Playback is no longer simulated; real streaming replaces the simulated state flag.

**Migration**: Use the play, stop, buffering and error behavior in `audio-playback`.

### Requirement: Playback state survives navigation

**Reason**: Playback is now tied to the `Reproductor` page in this phase; leaving the page stops audio, so the simulated state no longer survives navigation.

**Migration**: Use `audio-playback`. The selected station remains available, but playback must be started again after returning to `Reproductor`.
