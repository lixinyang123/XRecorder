;NSIS Modern User Interface
;Basic Example Script
;Written by Joost Verburg

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Recorder"
  OutFile "Recorder.exe"
  Unicode True

  ;Default installation folder
  InstallDir "$LOCALAPPDATA\Recorder"
  
  ;Get installation folder from registry if available
  ;InstallDirRegKey HKCU "Software\Modern UI Test" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  ;!insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\Modern UI\License.txt"
  ;!insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  File /nonfatal /r "Recorder\bin\Release\net7.0\win-x64\*.*"
  
  ;Store installation folder
  SetRegView 64
  WriteRegStr HKCR "Recorder" "" "URL:Recorder"
  WriteRegStr HKCR "Recorder" "URL Protocol" ""
  ;cmd /C "cd /D "D:\lixin\InProgress\Recorder\Recorder\bin\Release\net7.0\win-x64" & start /b Recorder.exe --open-url %1"
  WriteRegStr HKCR "Recorder\shell\open\command" "" 'cmd /C "cd /D "$LOCALAPPDATA/Recorder" & start /b Recorder.exe" "--open-url"  "--" "%1"'
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  ;LangString DESC_SecDummy ${LANG_ENGLISH} "A test section."

  ;Assign language strings to sections
  ;!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  ;!insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} $(DESC_SecDummy)
  ;!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...

  Delete "$INSTDIR\*.*"

  RMDir "$INSTDIR"

  DeleteRegKey /ifempty HKCR "Recorder"

SectionEnd
