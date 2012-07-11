@setlocal EnableExtensions

@cd "%~dp0.."

@set input_dir=%cd%\ChangeLog
@set output_dir=%cd%\_Build\ProductInfo
@set output_file=%output_dir%\ChangeLog.txt

@if not exist "%output_dir%" mkdir "%output_dir%"
@if exist "%output_file%" del "%output_file%"

@cd "%input_dir%"

@for /F "usebackq delims==" %%I in (`dir /B /O-N *.txt`) do @call :process %%I
@goto :end

:process
@set version=%~n1:
@set version=%version:_Z=%
@set version=%version:_= %
@echo Changes in %version%>> "%output_file%"
@echo.>> "%output_file%"
@for /F "usebackq delims==" %%I in ("%1") do @echo %%I>> "%output_file%"
@echo.>> "%output_file%"

:end
