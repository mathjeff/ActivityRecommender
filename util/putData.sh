cd $(dirname $0)
set -e
echo copying
cd
cd ActivityData
fileName="$1"
if [ "$fileName" == "" ]; then
  echo "Usage: $0 <filename>"
  exit 1
fi
adb push "$fileName" /storage/emulated/0/ActivityRecommender
echo copied "$fileName"
