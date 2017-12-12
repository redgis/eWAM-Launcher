Todo :

- expand environment variables to show the result in real time
- Remove Wyde-DLL : this should not be editable, and be set from the binaries set
- Find solution for binaries sets edition

- Document code

- Save / load session feature / button : implemented as a command ?

- general settings : 
   - reload last session (based on last session file opened, default is in %appdata%)
   
- importer : 
   have a list of predefined subfolders in settings, to lookup into, for each type of thing to import
   e.g., for binaries : bin/, bin/win32/debug/, bin/win32/release, bin/release/op, etc... or just recursively lookup for binaries and dlls ?
   
- Auto updater using Squirrel
- Process monitor looking up possible launchers working
   => use WYDE-TGV env var to find matches
- Give information about eWAM binaries versions / and if it is up to date
- Allow binaries updating
- Shouldn't we simply have a pool of eWAMs referenced by the various environments ?

[Test push on protected branch 2]