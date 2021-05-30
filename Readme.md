## SCI engine tools for resource editing and translating

[![build](https://github.com/deadman2000/sci_tools/actions/workflows/build.yml/badge.svg)](https://github.com/deadman2000/sci_tools/actions/workflows/build.yml) [![SCI_Lib on fuget.org](https://www.fuget.org/packages/SCI_Lib/badge.svg)](https://www.fuget.org/packages/SCI_Lib)

**SCI_Lib** - resource management library for SCI-games. 

**SCI_Tools** - executable utility. Scripting, editing, extracting, backend tasks, etc.

**SCI_Translator** - desktop application. Resource viewing, text search, extracting.


## SCI specifications

- http://wiki.scummvm.org/index.php/SCI/Specifications
- https://github.com/scummvm/scummvm/tree/master/engines/sci
- https://github.com/ValeryAnisimovsky/GameAudioPlayer/blob/master/Specs/SOL-AUD.txt


## TODO

### Talker detection

By export offsets get Talker insntance. Show it's view
By noun get Actor. Show it's view
Package method GetTalkerInfo(message number, talkerId)
Returns name, view(num, loop)