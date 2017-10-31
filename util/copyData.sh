set -e
echo copying
fileName="$(adb shell 'ls /storage/emulated/0/ActivityData-*.txt' | sort | tail -n 1 | head -c 56)"
destDir="temp/"
adb pull "$fileName" "$destDir"
echo copied "$fileName" to "$destDir"