@echo off
echo "Usage with no parameter, it creates 'latest' version, with 'newRelease' parameter it creates a new release tag"
:parse
set appVersion=latest
IF not "%~1"=="newRelease" GOTO endparse

for /f %%a in ('wmic os get localdatetime ^| find "."') do set datetime=%%a
set YYYY=%datetime:~0,4%
set MM=%datetime:~4,2%
set DD=%datetime:~6,2%
set formatted_date=%YYYY%%MM%%DD%
echo Date: %formatted_date%
set appVersion=R.%formatted_date%
:endparse
echo Version: %appVersion%
[Environment]::SetEnvironmentVariable("ps_version", "appVersion", "User")
set varCheck=[Environment]::GetEnvironmentVariable("ps_version", "User")
echo %varCheck%

echo docker build -f .\shopFlow.Main\Dockerfile . -t algiro/shop-flow-net:%appVersion%
docker build -f .\shopFlow.Main\Dockerfile . -t algiro/shop-flow-net:%appVersion%
docker push algiro/shop-flow-net:%appVersion%
