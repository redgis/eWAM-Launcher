
/!\ For the package-index.xml to be usable from eWAM Launcher, in IIS: remove all URL filterings and Hidden Segments. Add .* MIME type as application/octet-stream

To generate C# class from a reference XML file : XML => xsd => .cs  : https://stackoverflow.com/questions/3187444/convert-xml-string-to-object



# Todo


### Importing
- [ ] eWAM import : detect if 64 or 32 bits : https://superuser.com/questions/358434/how-to-check-if-a-binary-is-32-or-64-bit-on-windows
   https://stackoverflow.com/questions/1001404/check-if-unmanaged-dll-is-32-bit-or-64-bit

### Online repository
- [ ] Allow comparing local to online repo index, and suggest update binaries
- [ ] Allow launchers and environment configuration updates from online repo
- [ ] Improve package-index.xml format to make it lighter (decompose it in hierarchical architecture)

### Ergonomy
- [ ] Impprovement : Allow item reordering by drag&drop in listboxes and datagrids : 
   https://stackoverflow.com/questions/3350187/wpf-c-rearrange-items-in-listbox-via-drag-and-drop
   http://www.hardcodet.net/2009/03/moving-data-grid-rows-using-drag-and-drop
   http://www.eidias.com/blog/2014/8/15/movable-rows-in-wpf-datagrid
- [ ] Make it interoperable with VSCode : VSCode plugin could use eWAM Launcher environments to choose on which env one is working

### Code quality
- [ ] XML serialization : get rid of DataContract ? (But we would also loose json serialization ?)

### Process management
- [ ] Process monitor looking up possible launchers working
- [ ] Retrieve launch status with standard output / error output
   => use WYDE-TGV env var to find matches



# Changelog


## 1.0.0.63 - 2019/01/30
- [x] Add "Save configuration" as opposed to "Save configuration AS", with associated shortcuts

## 1.0.0.62 - 2019/01/30
- [x] Add ability to generate *.xwam, *.jswam, *.xenv, *.jsenv for all env / ewams
- [x] Add feature to Reset settings to defaults
- [x] Import "PATH=" to Additional Path, when importing environment
- [x] Add scrollbar to log window
- [x] Add link/button to log folder
- [x] Add link/button to settings folder
- [x] Add Author's Name
- [x] Add link to WydeWeb CommandLine
- [x] Updated Icon
- [x] Added VS2013 in VS list
- [x] Fixed category for setting "Search pathes for eWam binaries"

## 1.0.0.61 - 2019/01/22
- [x] Various fixes

## 1.0.0.53/54 - 2019/01/11
- [x] bug fix : error when importing a package

## 1.0.0.51 - 2019/01/09
- [x] Document code
- [x] Code cleanup 
- [x] Fix bug where packages might be displayed twice


## Previous versions

### Basic features
- [x] Implement WydeWeb configuration (Client configuration, server configuration)
- [x] Allow to generate .bat files corresponding to launchers
- [x] Save / load session feature / button : implemented as a command (?)
- [x] Add menu bar, items, associated commands (?)
- [x] Command / Menu to export an entire environment or binary set
- [x] Added logging feature
- [x] Show log in a window, in the UI
- [x] Backup configuration on close, with limited number of backup kept

### WydeWeb configuration
- [x] Import configuration from wNetConf.ini, wNetConf.xml
- [x] Configuration edition
- [x] Add missing "extension" in wNetConf XML definitions
- [x] Make a simpler object to avoid redundancy between client and server configuration (security, etc), simplify http tunnel configuration
- [x] Export a configuration to a wNetConf.ini, wNetConf.xml, of WydeWebAsAuto.html
- [x] Add automatic wNetConf import on environment pull (included in .xenv) and import ! (based on WYDE-NETCONF)
- [x] Wizard to automatically deploy ClickOnce / OCX with given configuration (maybe create a Wizard for the occasion)

### Ergonomy
- [x] Shouldn't we simply have a pool of eWAMs referenced by the various environments ?
- [x] Remove Wyde-DLL : this should not be editable, and be set from the binaries set
- [x] Allow item reordering using buttons
- [x] When importing an eWAM, ask if wants to import the associated environment
- [x] When importing an environment, ask if wants to import the associated ewam if not already exists
- [x] Add link to confluence commandline options in launchers page
- [x] Don't die on any exception : have global try/catch for each feature (launch, import, explore, change path, etc). Show dialog box with exception message.
- [x] Add "Open Console" button opening cmd with associated env var, in env-root. (In environment variables window, or in general)
- [/] Use MasterDetailsView ? https://docs.microsoft.com/en-us/windows/communitytoolkit/controls/masterdetailsview
- [x] Add a Systray icon. Right click => close. Make it work correctly (not working when window doesn't have focus)
- [x] Reduce /close to systray. 
- [x] Double-click on Systray icon reopens app
- [x] Allow only one instance

### Code quality
- [/] Replace / bind implemented "actions" to WPF commands <= not sure it's worse it...
- [x] XAML / WPF : How to create a UI for a specific type, and include it in another ? use pages ?
- [x] Mutualize code between OnConsoleExecuteLauncher and OnExecuteLauncher
- [x] Rewrite variable importing : sanitize wyde-root, env-root, wf-root, wyde-dll and path after importing, let user choose the right value from a list or set custom value
- [x] Rewrite environment and ewam importing functions : buggy and messy at the moment
   
### Improve importing
- [x] Clean up environment variables at import : remove WYDE-DLL (which will be set at launch)
- [x] Should we discard WYDE-ROOT, and use ewam basePath ?
- [x] have a list of predefined subfolders in settings, to lookup into, for each type of thing to import
   e.g., for binaries : bin/, bin/win32/debug/, bin/win32/release, bin/release/op, etc... or just recursively lookup for binaries and dlls ?
- [x] eWAM import : remove eWAM root prefix from binaries pathes : only keep the sub-folders path. e.g. CppDll\Win32\Debug
   
### General settings
- [x] Implement application settings class, make it serializable
- [x] have a list of folder in which to lookup when importing a new environment
- [x] reload last session (based on last session file opened, default is in %appdata%)
- [x] address of update server
- [x] Add "Additional pathes", and forbid "PATH" in environment variables
- [x] Add Section with VS2010, VS2012 path to be called if needed, find a way to enable that in an environment.
   See https://stackoverflow.com/questions/1842491/how-do-i-find-the-path-of-visual-studio-in-the-registry-using-python :
      - HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VC7
      - path + vcvars*.bat :
         - D:\Developpement\EDIs\Visual Studio 2008 Professional\VC\bin\vcvars32.bat
         - D:\Developpement\EDIs\Visual Studio 2008 Professional\VC\bin\amd64\vcvarsamd64.bat
         - D:\Developpement\EDIs\Visual Studio 2010 Premium\VC\bin\vcvars32.bat
         - D:\Developpement\EDIs\Visual Studio 2010 Premium\VC\bin\amd64\vcvars64.bat
         - D:\Developpement\EDIs\Visual Studio 2012 Premium\VC\bin\vcvars32.bat
         - D:\Developpement\EDIs\Visual Studio 2012 Premium\VC\bin\amd64\vcvars64.bat

### Binaries/environment deployment from Web repository
- [x] Have a repository index online with all eWAM binaries
- [x] Add a progress bar for each package pull started
- [x] Zip-Compress packages using any compression method available (store, deflate, lzma, bzip2)
- [x] Re-design using BackgroundWorker : supports progress and cancellation !
   - [x] Add ability to cancel an on-going download
- [x] Allow downloading only selected components
- [x] Add a progressbar for download and extraction
- [/] Use nuget packaging ?
- [/] Notifications : when clicked, open destination
      
### Auto update
- [x] Auto updater using Squirrel

### Environment from scratch
- [x] Allow download of entire environment binaries, from scratch
- [x] Set of basic launchers for eWam (wEwam class in embeded xml file resource, ready to be deserialized into a new object)

### Bugs
- [x] eWAM import not importing its own environment correctly
- [x] Launcher Delete button not setting selection correctly after deleting last element of the list
- [x] Environment import creates an additional new environment for the eWAM set, even if the eWAM already existed
- [x] Wynsure launcher doesn't work when using environment variables in cmmand line parameter (e.g. @%WYDE-ADMIN%\config.cfg)
- [x] Block adding WYDE-ROOT : set at launch to binary root path
- [x] When importing an eWAM, the associated environment, the binary set isn't set in environment
- [x] When changing eWAM, also change (or at least remove) all the binary set used in the launchers
- [x] Add path to Wyseman and WydeWeb.exe somewhere (at import of eWAM or environment, or in path, at launch)
- [x] Wynsure environment launchers not importing correctly after environment pull from online repo
- [x] Auto-Update doesn't seem to work correctly ...
- [x] Importing fails with error
- [x] Fixed WW configuration issue
- 
