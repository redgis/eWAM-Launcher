# Todo


![screen01](doc/screenshot-01.png)
![screen02](doc/screenshot-02.png)


## Basic features
- [ ] Implement WydeWeb configuration (Client configuration, server configuration)
- [ ] Allow to generate .bat files corresponding to launchers
- [ ] Save / load session feature / button : implemented as a command (?)
- [ ] Add menu bar, items, associated commands (?)
	
## Ergonomy
- [x] Shouldn't we simply have a pool of eWAMs referenced by the various environments ?
- [x] Remove Wyde-DLL : this should not be editable, and be set from the binaries set
- [x] Allow item reordering using buttons
- [ ] Impprovement : Allow item reordering by drag&drop in listboxes and datagrids : 
   https://stackoverflow.com/questions/3350187/wpf-c-rearrange-items-in-listbox-via-drag-and-drop
   http://www.hardcodet.net/2009/03/moving-data-grid-rows-using-drag-and-drop
   http://www.eidias.com/blog/2014/8/15/movable-rows-in-wpf-datagrid
	
## Code quality	
- [ ] Document code

## Process management
- [ ] Process monitor looking up possible launchers working
- [ ] Retrieve launch status with standard output / error output
	=> use WYDE-TGV env var to find matches
	
## Improve importing
- [ ] Also clean up environment variables at import : remove WYDE-DLL (which will be set at launch)
- [ ] have a list of predefined subfolders in settings, to lookup into, for each type of thing to import
	e.g., for binaries : bin/, bin/win32/debug/, bin/win32/release, bin/release/op, etc... or just recursively lookup for binaries and dlls ?
- [ ] eWAM import : detect if 64 or 32 bits : https://superuser.com/questions/358434/how-to-check-if-a-binary-is-32-or-64-bit-on-windows
	
## General settings
- [x] Implement application settings class, make it serializable
- [x] have a list of folder in which to lookup when importing a new environment
- [x] reload last session (based on last session file opened, default is in %appdata%)
- [x] address of update server
		
## Auto update
- [x] Auto updater using Squirrel
   Need to build package before publishing on website :
      nuget pack -OutputDirectory Squirrel\ Squirrel\Ewam.Launcher.1.0.33.nuspec && packages\squirrel.windows.1.7.8\tools\Squirrel.exe --releasify Squirrel\Ewam.Launcher.1.0.33.nupkg --releaseDir=Squirrel\Release\ --packagesDir=packages\
- [ ] Give information about eWAM binaries versions / and if it is up to date
- [ ] Allow binaries update from remote server
- [ ] Allow pulling a new version of entire ewam binaries

## XAML / WPF / .NET things to learn
- [ ] How to create a UI for a specific type, and include it in another ? use pages ?

## Visual Studio environment
- [ ] Add Section with VS2010, VS2012 path to be called if needed

## Binaries deployment from Web repository
- [ ] Have a repository index online with all eWAM binaries
- [ ] Allow comparing local environment and suggest update binaries

## Environment from scratch
- [ ] Allow download of entire environment binaries, from scratch
- [ ] Set of basic launchers for eWam (wEwam class in embeded xml file resource, ready to be deserialized into a new object)

## Bugs
- [x] eWAM import not importing its own environment correctly
- [ ] Launcher Delete button not setting selection correctly after deleting last element of the list


https://youtu.be/rDjrOaoHz9s
