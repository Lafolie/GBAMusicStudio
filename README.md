# Kermalis's GBA Music Studio

A tool that is designed to be a Sappy replacement as well as support different game engines.

![Preview](https://i.imgur.com/ohBwyF0.gif)

----
# Some Advantages Over Sappy:
* Pause button & song position changing
* You can play the in-game instruments with a MIDI-in keyboard
* You can view and edit track events
* You can import MIDI files without having to convert them yourself
* ~~You can save the voice tables to SF2 soundfont files~~ (Currently disabled)
* You can save songs to ASM S files
* The UI scales to the desired window size
* You can see representation of notes being played
* It is not intimidating to use
* Support for multiple song tables
* Support for "Mario & Luigi: Superstar Saga"
* Support for "Golden Sun" synth instruments

----
# To Do:
## M4A / MP2K Engine
* Add Golden Sun 2 reverb effect
* Add reverse playback
* Add SquareWave sweeping
* Add "note off with noise" for SquareWaves
* XCMD command
* Repeat command
* Nested PATT (3 is the maximum)
* Support pret dissassembly projects
* Running status in song disassembler

## Mario & Luigi: Superstar Saga Engine
* Properly implement "free notes"
* Voice table - Find out the last 4 bytes in voice entry struct
* Voice table - Figure out squares (190s activate when in bottom 2/1 tracks... but only for some songs)
* MIDI saving

## General
* Add "Metroid Fusion" & "Metroid: Zero Mission" engine information
* MIDI saving - preview the MIDI with the Sequencer class
* MIDI saving - UI with saving options, such as remapping
* Fix voicetable saver
* Add playing playlist from Games.yaml and fading out after a configurable amount of loops
* Maybe a nice waveform
* Exception handling for invalid config
* Let on-screen piano play notes or interact with MIDI keyboard
* Remove "private set" in config and add saving of config
* Default remap voice
* Offset for the event you're editing
* Put the event parameter as text in the parameter name label, so there is reference to the original value or in MODT/Note's cases, text representation
* Buttons in the taskbar like with most music players
* Tempo numerical (it fits)
* Detachable piano
* Help dialog that explains the commands for each format
* If I go insane I'll support the MOD music format

----
# Special Thanks To:
* Ipatix [(And his GBA music player)](https://github.com/ipatix/agbplay/)
* tuku473
* Bregalad
* mimi
* Jesse (jelle)
* SomeShrug