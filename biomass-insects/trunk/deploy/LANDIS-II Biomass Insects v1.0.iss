#define PackageName      "Biomass Insects"
#define PackageNameLong  "Biomass Insects"
#define Version          "1.0"
#define ReleaseType      "official"
#define ReleaseNumber    "1"

#define CoreVersion      "5.1"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section) v2.iss"

;#include "..\package (Setup section).iss"


[Files]

; Dynamic Fire Fuel System v1.0 plug-in and auxiliary libs (Troschuetz Random)
Source: {#LandisBuildDir}\dynamic-biomass-fuels\build\release\Landis.DynFuelsBiomass.dll; DestDir: {app}\bin

Source: docs\LANDIS-II Dynamic Biomass Fuel System v1.0 User Guide.pdf; DestDir: {app}\doc

#define DynFuelSys "Dynamic Biomass Fuels 1.0.txt"
Source: {#DynFuelSys}; DestDir: {#LandisPlugInDir}

; All the example input-files for the in examples\dynamic-fire-fuel-system
Source: {#LandisBuildDir}\dynamic-biomass-fuels\deploy\examples\*; DestDir: {app}\examples\dynamic-fire-fuel-system; Flags: recursesubdirs

[Run]
;; Run plug-in admin tool to add entries for each plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "add ""{#DynFuelSys}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove entries for each plug-in
Filename: {#PlugInAdminTool}; Parameters: "remove ""Dynamic Biomass Fuel System"" "; WorkingDir: {#LandisPlugInDir}

[Code]
#include AddBackslash(LandisDeployDir) + "package (Code section).iss"

//-----------------------------------------------------------------------------

function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
begin
  // Remove the plug-in name from database
  if StartsWith(currentVersion.Version, '1.0') then
    begin
      Exec('{#PlugInAdminTool}', 'remove "Dynamic Biomass Fuel System"',
           ExtractFilePath('{#PlugInAdminTool}'),
		   SW_HIDE, ewWaitUntilTerminated, Result);
	end
  else
    Result := 0;
end;

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
