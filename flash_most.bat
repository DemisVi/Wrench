@echo off

@REM set logfilename=%date:~6,4%%date:~3,2%%date:~0,2%%time:~0,2%%time:~3,2%%time:~6,2%_fastboot.log
@REM  >> %logfilename%

adb wait-for-device
adb reboot bootloader

fastboot flash aboot .\appsboot.mbn
fastboot flash rpm .\rpm.mbn
fastboot flash sbl .\sbl1.mbn
fastboot flash tz .\tz.mbn
fastboot flash modem .\modem.img
fastboot flash boot .\boot.img
fastboot flash system .\system.img
fastboot flash recovery  .\recovery.img
fastboot flash recoveryfs .\recoveryfs.img
fastboot reboot