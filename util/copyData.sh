cd $(dirname $0)
set -e
echo copying
fileName="$(adb shell 'ls /storage/emulated/0/ActivityData-*.txt' | sort | tail -n 1 | head -c 56)"
destDir="ActivityData"
cd
cd ActivityData
adb pull "$fileName" .
echo copied "$fileName" to "~/$destDir/$(basename $fileName)"