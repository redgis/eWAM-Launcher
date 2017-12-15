#Todo

##Basic features
	- Implement WydeWeb configuration (Client configuration, server configuration)
	- Allow to generate .bat files corresponding to launchers
	- Save / load session feature / button : implemented as a command (?)
	- Add menu bar, items, associated commands (?)
	
##Ergonomy
	- Shouldn't we simply have a pool of eWAMs referenced by the various environments ?
	- Remove Wyde-DLL : this should not be editable, and be set from the binaries set
	
##Code quality	
	- Document code

##Process management
	- Process monitor looking up possible launchers working
		=> use WYDE-TGV env var to find matches
	
##Improve importing
	- Also clean up environment variables at import : remove WYDE-DLL (which will be set at launch)
	- have a list of predefined subfolders in settings, to lookup into, for each type of thing to import
		e.g., for binaries : bin/, bin/win32/debug/, bin/win32/release, bin/release/op, etc... or just recursively lookup for binaries and dlls ?
	
##General settings
	- Implement application settings class, make it serializable
	- have a list of folder in which to lookup when importing a new environment
   - reload last session (based on last session file opened, default is in %appdata%)
	- address of update server
		
##Auto update
	- Auto updater using Squirrel
	- Give information about eWAM binaries versions / and if it is up to date
	- Allow binaries update from remote server
	- Allow pulling a new version of entire ewam binaries