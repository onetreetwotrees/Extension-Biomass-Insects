#define PackageName      "Biomass Insects"
#define PackageNameLong  "Biomass Insects"
#define Version          "2.0"
#define ReleaseType      "official"
#define ReleaseNumber    "2"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section) v6.0.iss"

[Files]

; Biomass Insects
Source: C:\Program Files\LANDIS-II\6.0\bin\Landis.Extension.BiomassInsects.dll; DestDir: {app}\bin; Flags: replacesameversion

Source: docs\LANDIS-II Insect Defoliation v2.0 User Guide.pdf; DestDir: {app}\doc
Source: examples\*; DestDir: {app}\examples\biomass-insects; Flags: recursesubdirs


#define BioBugs "Biomass Insects 2.0.txt"
Source: {#BioBugs}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add entries for each plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Biomass Insects"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#BioBugs}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove entries for each plug-in

[Code]
#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Code section) v3.iss"

//-----------------------------------------------------------------------------

function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
begin
end;

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
