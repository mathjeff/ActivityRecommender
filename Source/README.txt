ActivityRecommender
By Jeff Gaston

The ActivityRecommender was created to answer the question “What should I be doing now?” based on user-provided data. Humans can be both forgetful and irrational; computers are neither.

Column 1: Enter Activities to Choose From
One form of user-input is for the user to enter into the system the name of an activity to be considered in the future. The program will only suggest activities that it has been informed of in this way.
Tip: When you are typing an activity name, you’ll notice that there is a white or yellow or red box below what you are typing. If the box is red, that means the program doesn’t recognize what you are typing and can’t offer any autocomplete suggestions. It does not mean that what you are typing is wrong; in fact, if you enter an activity that it was not previously aware of, then it will add this activity to the system and will suggest it for autocomplete in the future. If the box is yellow, then the program has an idea of what you’re trying to type, and you may push [tab] or [enter] to automatically fill in what you see there.
Each activity may also be optionally assigned one or more super categories to which it belongs. For instance, the activity named Frisbee may be an example of the activity named Exercise, which may belong to the activity named Useful. Frisbee may also belong to the activity named Fun.
The program thusly builds an inheritance hierarchy, which allows it to better predict ratings of activities based on similar activities.
These super categories may be thought of as tags, except that they may be assigned to other super categories and so on.
Note: This information (telling which activities are subtypes of others) is stored in ActivityInheritances.txt in an easy-to-read XML format. If you are familiar with XML you may edit it in a text editor.

Column 2: Type What You’ve Been Doing
The other primary method of user input is to type in the name of an activity that the user performed, along with the starting and ending date and time, and optionally a numerical rating from 0 to 1, indicating the value of having performed that activity between those times. The data entered in this column in stored in ActivityRatings.txt.

If you recently pushed the “Suggest” button in column 3, you may want to push the “Autofill” button here to automatically fill in a good guess for the correct dates and activity name. The information about the default StartDate is stored in TemporaryData.txt.

Column 3: Get Suggestions
After having supplied some data to the program, the user may ask for a suggestion by pushing the button labeled “Suggest”.

Note that the act of requesting two suggestions consecutively is an indication that the first suggestion was unsatisfactory. Therefore, if the user requests a suggestion and does not enter any data before requesting another, then the program automatically generates a low rating to assign to previously suggested activity.

Column 4: View Statistics
After having used this program for a while, the user may want to see visual representations of the data that has been entered. Simply type in an activity name and push “Visualize” to see the graphs.
The top graph will show the ratings of that activity over time, and the bottom graph will show the total amount of time you’ve spent on that activity over time. Each activity’s data includes all of its child activities and their children etc.
