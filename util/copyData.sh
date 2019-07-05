cd $(dirname $0)
set -e
echo copying
fileName="$(adb shell 'ls /storage/emulated/0/ActivityData-2*.txt' | sort | tail -n 1 | head -c 56)"
destDir="ActivityData"
cd
cd ActivityData
adb pull "$fileName" .
destPath="$HOME/$destDir/$(basename $fileName)"
echo
echo copied "$fileName" to "$destPath"
echo
tail -n 10 "$destPath"
