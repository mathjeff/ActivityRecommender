This file describes the ways in which ActivityRecommender shares or doesn't share user data.

Essentially, ActivityRecommender does not share user data and does not even use the internet.
   * Exceptions:
       * If the user initiates an action that the user clearly expects to require the internet, then as part of completing that action, ActivityRecommender may cause the user's device to implement the network portion of that action
           * Examples:
               * If the user pushes the Open an Issue button, then ActivityRecommender asks the device's web browser to open the appropriate web page
               * When the user asks ActivityRecommender to import their data, it may be possible for the user to select a location on the network from which to import
   * Note that if another entity running on the device makes backups or copies of application data, it is possible that those entities could share your ActivityRecommender data

