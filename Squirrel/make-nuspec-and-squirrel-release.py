import os

search = open("Properties/AssemblyInfo.cs")
for line in search:
	if line.startswith("[assembly: AssemblyFileVersion(\""):
		version = line.split('\"')[1]
		numbers = version.split(".")
		version = numbers[0] + '.' + numbers[1] + '.' + numbers[2]
		targetSpec = "Squirrel\\Ewam.Launcher." + version + ".nuspec"
		targetPkg = "Squirrel\\Ewam.Launcher." + version + ".nupkg"
		if os.path.isfile(targetSpec):
			os.remove(targetSpec)
			
		with open("Squirrel\\Ewam.Launcher.VERSION.nuspec-for-integration") as sourceFile:
			with open(targetSpec, 'w') as targetFile:
				for sourceLine in sourceFile:
					targetFile.write(sourceLine.replace("<version></version>", "<version>" + version + "</version>"))
			targetFile.close()
		sourceFile.close()
		
		os.system("nuget pack -Version " + version + " -OutputDirectory Squirrel\\ Squirrel\\Ewam.Launcher." + version + ".nuspec")
		os.system("packages\\squirrel.windows.1.9.0\\tools\\Squirrel.exe --releasify Squirrel\\Ewam.Launcher." + version + ".nupkg --releaseDir=Squirrel\\Releases\\ --packagesDir=packages\\")
		os.system("git add " + targetSpec)
		os.system("git add " + targetPkg)
		os.system("git add " + "Squirrel\\Releases\\Ewam.Launcher-" + version + "-delta.nupkg")
		os.system("git add " + "Squirrel\\Releases\\Ewam.Launcher-" + version + "-full.nupkg")
		os.system("git commit -m \"Squirrel package from integrator\"")