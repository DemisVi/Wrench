@REM @echo off

@REM set logfilename=%date:~6,4%%date:~3,2%%date:~0,2%%time:~0,2%%time:~3,2%%time:~6,2%_adb.log
@REM  >> %logfilename%

adb wait-for-device
adb push factory.cfg /data
adb reboot
