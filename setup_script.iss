; Inno Setup Deployment Script for Nexoris POS
; Compatible with Inno Setup compiler v5 or v6.

[Setup]
AppName=Nexoris POS
AppVersion=1.0.0
AppPublisher=Nexoris
DefaultDirName={autopf}\Nexoris POS
DefaultGroupName=Nexoris POS
OutputDir=.\OutputInstaller
OutputBaseFilename=Nexoris_POS_Setup
Compression=lzma
SolidCompression=yes
SetupIconFile=
PrivilegesRequired=admin
DisableWelcomePage=no
DisableDirPage=no
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Copy all compiled binaries and assets recursively from the build folder
Source: "c:\AbrarNexorisLatestproj\AbrarNexoris\PosBranch-Win\bin\Debug\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Nexoris POS"; Filename: "{app}\NexorisPOS.exe"
Name: "{commondesktop}\Nexoris POS"; Filename: "{app}\NexorisPOS.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\NexorisPOS.exe"; Description: "{cm:LaunchProgram,Nexoris POS}"; Flags: nowait postinstall skipifsilent

[Code]
// Helper function to create default config connection file post-install if it doesn't exist.
procedure CurStepChanged(CurStep: TSetupStep);
var
  ConfigDir: string;
  ConfigFile: string;
  DefaultConfig: string;
begin
  if CurStep = ssPostInstall then
  begin
    ConfigDir := 'C:\Connection';
    ConfigFile := ConfigDir + '\Config.txt';
    
    // Default connection string template
    DefaultConfig := '192.168.1.232\SQLEXPRESS;RambaiTest;sa;Abrar@123;';

    try
      // Ensure Connection Directory exists
      if not DirExists(ConfigDir) then
      begin
        if CreateDir(ConfigDir) then
          Log('Created folder C:\Connection')
        else
          Log('Failed to create folder C:\Connection');
      end;

      // Ensure Config file exists, write template if not present
      if not FileExists(ConfigFile) then
      begin
        if SaveStringToFile(ConfigFile, DefaultConfig, False) then
          Log('Successfully wrote default config to C:\Connection\Config.txt')
        else
          Log('Failed to write default config to C:\Connection\Config.txt');
      end
      else
      begin
        Log('Config file already exists. Preserving current connection configuration.');
      end;
    except
      MsgBox('An unexpected error occurred while creating database connection configuration files. Please configure C:\Connection\Config.txt manually.', mbError, MB_OK);
    end;
  end;
end;
