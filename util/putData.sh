cd $(dirname $0)
set -e
echo copying
fileName="$(ls temp/ActivityData-*.txt | sort | tail -n 1 | head -c 56)"
adb push "$fileName" /storage/emulated/0/
echo copied "$fileName"