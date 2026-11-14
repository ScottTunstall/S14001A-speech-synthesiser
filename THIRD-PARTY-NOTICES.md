# Third-party notices

## MAME S14001A emulation

The files in `src/S14001ASpeak/Synthesis` that start with a "Ported from MAME" or "Values taken from MAME"
header are a C# port of `src/devices/sound/s14001a.cpp` and `s14001a.h` from MAME
(https://github.com/mamedev/mame, master branch as of commit d0f1c15, April 2025), distributed under the
BSD 3-Clause license:

```
Copyright (c) Ed Bernard, Jonathan Gevaryahu, hap
Thanks to Kevin Horton
All rights reserved.
```

## MAME Berzerk driver

`src/S14001ASpeak/Speech/BoardTiming.cs` takes the speech clock values of Stern's VSU-1000 board from
`src/mame/stern/berzerk.cpp` in MAME, distributed under the BSD 3-Clause license:

```
Copyright (c) Zsolt Vasvari, Aaron Giles, R. Belmont, Jonathan Gevaryahu
All rights reserved.
```

Both are distributed under these terms:

```
Redistribution and use in source and binary forms, with or without modification, are permitted
provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of
   conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of
   conditions and the following disclaimer in the documentation and/or other materials provided
   with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to
   endorse or promote products derived from this software without specific prior written
   permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## OggVorbisEncoder

Ogg Vorbis output uses the [OggVorbisEncoder](https://github.com/SteveLillis/.NET-Ogg-Vorbis-Encoder) NuGet
package (a managed port of the Xiph.Org libvorbis encoder), distributed under the MIT license. Test projects
decode the files with [NVorbis](https://github.com/NVorbis/NVorbis), also MIT.

## System.Speech

On .NET 10 the Windows speech voices are reached through Microsoft's
[System.Speech](https://www.nuget.org/packages/System.Speech) NuGet package, MIT license. On .NET Framework
4.8 it is part of the framework.
