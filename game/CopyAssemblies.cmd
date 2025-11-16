@echo off

SET realGameDir=c:\Program Files (x86)\Steam\steamapps\common\Railroader\Railroader_Data\Managed
SET installerDir=c:\projects\RailManager\src\RailManagerInstaller\Assemblies

echo.
md Railroader_Data\Managed\

echo.
echo Copying Game Files ...
xcopy "%realGameDir%" Railroader_Data\Managed /E /I /Q /Y
echo.

echo Copying Manager Files ...
for /F "tokens=*" %%A in (ManagerFiles.txt) do xcopy "%installerDir%\%%A" Railroader_Data\Managed\%%A /Y /-I /Q
echo.
