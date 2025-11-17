@echo off
echo ^<Project^> > Paths.user
echo   ^<PropertyGroup^> >> Paths.user
echo     ^<!-- Directory that the game (Railroader.exe) is in --^> >> Paths.user
echo     ^<GameDir^>c:\Program Files (x86)\Steam\steamapps\common\Railroader^</GameDir^> >> Paths.user
echo   ^</PropertyGroup^> >> Paths.user
echo ^</Project^> >> Paths.user

xcopy Paths.user mods\Paths.user /Y

set launchSettings=src\RailManagerInstaller\Properties\launchSettings.json

echo { > %launchSettings%
echo   "profiles": { >> %launchSettings%
echo     "Railroader.ModsManager.Installer": { >> %launchSettings%
echo       "commandName": "Project", >> %launchSettings%
echo       "workingDirectory": "c:\\Program Files (x86)\\Steam\\steamapps\\common\\Railroader" >> %launchSettings%
echo     } >> %launchSettings%
echo   } >> %launchSettings%
echo } >> %launchSettings%