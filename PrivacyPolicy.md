This file describes the ways in which ActivityRecommender shares and stores user data

Summary: ActivityRecommender may store data until you delete or uninstall it. It doesn't share unless you ask it to.

Data Storage Locations
  * Internal storage
    * ActivityRecommender only saves user data internally in the AppDataDirectory unless you prompt it to export to another location
      * This means if you uninstall the app, the data that ActivityRecommender saved is deleted
      * Another way to clear the app data without uninstalling the app is to use the Import feature and choose an empty file
      * Note that if another entity on your device has access to this data, it could be possible for that other entity to share your ActivityRecommender data
  * Exporting to other locations
    * ActivityRecommender does not share user data unless you explicitly request it to
      * If you ask ActivityRecommender to export your data, then you choose the location to share to, at which point ActivityRecommender sends a copy of your data to that location
      * Note that if another entity on your device has access to this data, it could be possible for that other entity to share this copy of your ActivityRecommender data

Data Storage Duration
  * ActivityRecommender does not automatically delete data intentionally
    * If you would like your data to be deleted, you may delete it from the appropriate location described above
  * Occasionally the process of installing a new version of the app might delete data
    * This may be more common when installing a development version of the app
    * You may wish to export a backup of your data before installing a new version of the app

ActivityRecommender doesn't even use the internet unless you ask it to
  * If you choose to import data from a source on the network, ActivityRecommender will read from that file
  * If you choose to export data to a source on the network, ActivityRecommender will save a file there
  * If you push the Open an Issue button, then ActivityRecommender asks the device's web browser to open the appropriate web page
