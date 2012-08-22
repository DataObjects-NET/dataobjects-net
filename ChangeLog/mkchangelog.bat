@setlocal EnableExtensions

@cd "%~dp0.."

@set input_dir=%cd%\ChangeLog
@set output_dir=%cd%\_Build\ProductInfo
@set change_log_file=%output_dir%\ChangeLog.txt
@set notes_file=%output_dir%\ReleaseNotes.txt

@if not exist "%output_dir%" mkdir "%output_dir%"
@if exist "%change_log_file%" del "%change_log_file%"
@if exist "%notes_file%" del "%notes_file%"

@cd "%input_dir%"

@for /F "usebackq delims==" %%I in (`dir /B /O-N *.txt`) do @call :process %%I
@goto :end

:process
@if not exist "%notes_file%" copy "%1" "%notes_file%"
@set version=%~n1:
@set version=%version:_Z=%
@set version=%version:_= %
@echo Changes in %version%>> "%change_log_file%"
@echo.>> "%change_log_file%"
@for /F "usebackq delims==" %%I in ("%1") do @echo %%I>> "%change_log_file%"
@echo.>> "%change_log_file%"

:end
