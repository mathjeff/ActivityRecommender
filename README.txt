This file describes the ActivityRecommender, created by Jeff Gaston

The ActivityRecommender suggests an activity for the user to be doing at the current moment, based on data provided by the user.


One form of user-input is for the user to enter into the system the name of an activity to be considered in the future.
The program will only suggest activities that it has been informed of in this way.
Each activity may also be optionally assigned one or more super categories to which it belongs.
For instance, the activity named Frisbee may be an example of the activity named Exercise, which may belong to the activity named Useful.
Frisbee may also belong to the activity named Fun.
The program thusly builds an inheritance hierarchy, which allows it to better predict ratings of activities based on similar activities.

The other primary method of user input is to type in the name of an activity that the user performed, along with the starting and ending date and time, and optionally a numerical rating from 0 to 1, 
indicating the value of having performed that activity between those times.


After having supplied some data to the program, the user may ask for a suggestion.

Note that the act of requesting two suggestions consecutively is an indication that the first suggestion was unsatisfactory.
Therefore, if the user requests a suggestion and does not enter any data before requesting another, then the program automatically generates a low rating to assign to previously suggested activity.


Note that the program uses 3 data files:
ActivityInheritances.txt
ActivityRatings.txt
TemporaryData.txt
Each is made human-readable on purpose, and they can be edited in a program such as Notepad if the user accidentally enters incorrect data.
