cd $(dirname $0)
set -e
echo copying
cd
cd ActivityData
fileName="$(ls ActivityData-*.txt | sort | tail -n 1 | head -c 56)"
adb push "$fileName" /storage/emulated/0/ActivityRecommender
echo copied "$fileName"
