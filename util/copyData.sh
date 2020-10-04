cd $(dirname $0)
set -e
echo copying
fileName="$(adb shell 'ls /storage/emulated/0/ActivityRecommender/ActivityData-2*.txt' | sort | tail -n 1 | tr -d '\r' | tr -d '\n')"
echo filename is $fileName
destDir="ActivityData"
cd
cd ActivityData
adb pull "$fileName" .
destPath="$HOME/$destDir/$(basename $fileName)"
echo
echo copied "$fileName" to "$destPath"
echo
tail -n 10 "$destPath"
