set -e

function usage() {
	echo "Usage: $0 <input.csv> [<input2.csv>...]"
	echo "Given one or more csv files describing participation history, generates one xml file suitable to import in ActivityRecommender"
	exit 1
}

if [ "$1" == "" ]; then
	usage
fi

fullCsvPath=/tmp/concatenated.csv
expectedHeader=".User,Email,Client,Project,Task,Description,Billable,Start date,Start time,End date,End time,Duration,Tags,Amount ()"
function concatInputs() {
	echo -n > "$fullCsvPath"
	while [ "$1" != "" ]; do
		inputFile="$1"
		actualHeader="$(head -n 1 $inputFile)"
		if echo $actualHeader | grep "^${expectedHeader}$" > /dev/null; then
			tac "$inputFile" | grep -v "^${expectedHeader}$" >> "$fullCsvPath"
		else
			echo "Could not parse file $inputFile."
			echo "Got header:      '$actualHeader' ($(echo $actualHeader | wc -c) chars)"
			echo "Expected header: '$expectedHeader' ($(echo $expectedHeader | wc -c) chars)"
			return 1
		fi
		shift
	done
}
concatInputs "$@"

sanitizedPath=/tmp/sanitized.csv
function sanitize() {
	sed 's/\("[^,]*\),\(.*"\)/\1 and \2/g' "$fullCsvPath" | sed 's/\("[^,]*\),\(.*"\)/\1 and \2/g' > "$sanitizedPath"
}
sanitize

reformattedPath=/tmp/reformatted.csv
function reformatFields() {
	sed 's/^[^,]*,[^,]*,[^,]*,\([^,]*\),[^,]*,\([^,]*\),[^,]*,\([^,]*\),\([^,]*\),\([^,]*\),\([^,]*\),[^,]*,\([^,]*\),\([^,]*\)/\1,\2,\3T\4,\5T\6/' "$sanitizedPath" > "$reformattedPath"
}
reformatFields

outputPath=/tmp/import.txt
echo -n > "$outputPath"
# Each Project is a parent of the activity indicated in the Description
function outputProjectInheritances() {
	cat "$reformattedPath" | sed 's|\([^,]*\),\([^,]*\),\([^,]*\),\([^,]*\)|<Inheritance><Child><Name>\2</Name></Child><Parent><Name>\1</Name></Parent></Inheritance>|g' | sort | uniq >> "$outputPath"
}
outputProjectInheritances

#Each Description of the form "A: B" means A is a parent of "A: B"
function outputColonInheritances() {
	cat "$reformattedPath" | grep ': ' | sed 's|[^,]*,\([^,]*\): \([^,]*\),\([^,]*\),\([^,]*\)|<Inheritance><Child><Name>\1: \2</Name></Child><Parent><Name>\1</Name></Parent></Inheritance>|g' | sort | uniq >> "$outputPath"
}
outputColonInheritances

#Each Tag is a parent of the activity indicated in the Description
#function outputTagInheritances() {
#	cat "$reformattedPath" | 
#}

function outputParticipations() {
	cat "$reformattedPath" | sed 's|\([^,]*\),\([^,]*\),\([^,]*\),\([^,]*\)|<Participation><Activity><Name>\2</Name></Activity><StartDate>\3</StartDate><EndDate>\4</EndDate></Participation>|' >> "$outputPath"
}
outputParticipations

echo "Created $outputPath"
