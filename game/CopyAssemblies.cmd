@echo off

SET realGameDir=c:\Program Files (x86)\Steam\steamapps\common\Railroader\Railroader_Data\Managed

echo.
md Railroader_Data\Managed\

echo.
echo Copying Game Files ...
xcopy "%realGameDir%" Railroader_Data\Managed /E /I /Q /Y
echo.
