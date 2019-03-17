set -e
function usage() {
  echo "Usage: $0 <filePath of ActivityData file to process"
  echo ""
  echo "Extracts efficiency data provided in the given file"
  return 1
}

inputPath="$1"
if [ "$inputPath" == "" ]; then
  usage
fi
echo "Pre-task DateTime, Pre-task efficiency, Post-task DateTime, Post-task efficiency"
grep "<Efficiency>.*</Efficiency>" $inputPath | sed 's|.*<Participation>.*<StartDate>\([^<]*\)</StartDate>.*<Efficiency><Value>\([^<]*\)</Value>.*<Earlier>.*<StartDate>\([^<]*\).*<Value>\([^<]*\).*|\3,\4,\1,\2|'
