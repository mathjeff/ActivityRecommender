using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;

// the EngineTester makes an Engine and calculates its average squared error
// The first time (on 2012-1-15) that I calculated the Root(Mean(Squared(Error))), it was about 0.327

/*
Note that at some unknown time, the absolute ratings were removed from the error calculation because they were both unimportant and also easy to predict
The implementation of the RelativeRating was done on 2012-01-06, so it happened some time after that

The latest results (on 2012-2-17) using a bunch of independent, combined predictions is:
typicalScoreError = 0.147785971030127
typicalProbabilityError = 0.470028763507115

The latest results (on 2012-2-18) using a multidimensional interpolator (which is slower) are:
typicalScoreError = 0.113297119720235
typicalProbabilityError = 0.470853382639463

latest results (on 2012-2-20) using a slightly faster version are:
typicalScoreError = 0.118867951358166
typicalProbabilityError = 0.472384299236938

latest results (on 2012-2-25) using a slightly faster version (where the Interpolator skips any splits that will be overwritten by later splits) are:
typicalScoreError = 0.118790161274347
typicalProbabilityError = 0.470607845933648     // this is the error rate we'd expect if each participation probability was 0.669 (or 0.331)

latest results (on 2012-4-10) simply after having acquired additional data (due to the passage of time)
typicalScoreError = 0.131244338310556
typicalProbabilityError = 0.456090786674865
 
latest results (on 2012-4-10) after switching back to getValueExponentially in the ParticipationProgression
typicalScoreError = 0.130215012360051
typicalProbabilityError = 0.456090786674865     // this is the error rate we'd expect if each participation probability was 0.705 (or 0.295)
// I think that my increased usage of the RelativeRating is increasing the information content of my ratings, and decreasing the prediction accuracy
// I think the increased data is improving the accuracy of the participation probability


latest results (on 2012-4-11) after removing from the averages and participations that were known to not have been suggested
typicalScoreError = 0.129996871657446
typicalProbabilityError = 0.466402441220659

latest results (on 2012-4-13) after acquiring new data
typicalScoreError = 0.129628174126225
typicalProbabilityError = 0.466490032356424

latest results (on 2012-4-13) after adjusting the AdaptiveLinearInterpolator's input-splitting terminating condition to involve counting the number of points again,
 rather than looking at the stdDev of the inputs
typicalScoreError = 0.132018880366107
typicalProbabilityError = 0.448369070940053

latest results (on 2012-6-16) after acquiring new data 
typicalScoreError = 0.132725069636051
typicalProbabilityError = 0.462996444030975

latest results (on 2012-6-16) after changing the interpolator's input-splitting-termination-criteria to be based on the number of points, not the size of the inputs
The problem was that if coordinate was a constant, then the product of the input variations was zero
typicalScoreError = 0.135236469558133
typicalProbabilityError = 0.44306302626509

 
latest results (on 2012-8-30) after acquiring new data 
typicalScoreError = 0.140364754582535
typicalProbabilityError = 0.443507071557773

latest results (on 2012-8-30) after adjusting the interpolator to do a more sensible job when given data outside the promised input range
typicalScoreError = 0.140700447417066
typicalProbabilityError = 0.442070071914349

 
latest results (on 2012-8-31) after telling the interpolator to update more often
typicalScoreError = 0.140585869049799
typicalProbabilityError = 0.439053966981607 
equivalentProbability = 0.739231298282047

latest results (on 2012-10-7) after acquiring more data:
typicalScoreError = 0.147389266231023
typicalProbabilityError = 0.440218252424753
equivalentProbability = 0.737082032706184

latest results (on 2012-10-7) after using some of the unprompted participations as fake, prompted participations
typicalScoreError = 0.147260241883526
typicalProbabilityError = 0.430046711300231
equivalentProbability = 0.755068277329534

latest results (on 2012-10-21) after updating the engine to predict the (exponentially weighted) future ratings of all activities, but without
  having updated the EngineTester to adjust the target values accordingly:
typicalScoreError = 0.150655861416498
typicalProbabilityError = 0.431639712860378
equivalentProbability = 0.752363147630177
 

latest results (on 2012-11-17) after acquiring new data and then updating the engine to distinguish between expected immediate rating and expected overall future 
 rating (and computing error based on expected immediate rating)
typicalScoreError = 0.147185861338085
equivalentProbability = 0.744923948566977

latest results (on 2012-11-17) after 1. Adjusting the engine back to compute the difference between the predicted score and the updated score that is based on the
 relative rating that generated it, and 2. Having the engine compute the error of the long-term prediction
typical longtermPrediction error = 0.273375212068489
typicalScoreError = 0.163789122061962
equivalentProbability = 0.744923948566977


results (on 2012-11-17) if I have the EngineTester skip any predictions having weight of 0
typical longtermPrediction error = 0.0621895084186268
typicalScoreError = 0.16571109025582
typicalProbabilityError = 0.435903956644535
equivalentProbability = 0.744923948566977

 
results (on 2012-11-17) after having re-added the small amount of extra error to all predictions (which mattered for predictions having weight 0)
typical longtermPrediction error = 0.105942853885603
typicalScoreError = 0.16571109025582
typicalProbabilityError = 0.435903956644535
equivalentProbability = 0.744923948566977
 
results (on 2013-6-29) with new data, and also a new metric that weights predictions more heavily as the participation probability approaches 0 or 1
typicalScoreError = 0.181744174188761
typicalProbabilityError = 0.366714378232788
equivalentProbability = 0.839883163450824
weightedProbabilityScore = 0.320958529681715
equivalentWeightedProbability = 0.922818404503476
typical longtermPrediction error = 0.14057449369587

results (on 2014-1-05) with new data and after multiplying by 4 the frequency at which RatingSummaries get updated
typical longtermPrediction error = 0.139459605031934
typicalScoreError = 0.17654153306917
typicalProbabilityError = 0.360819685054036
equivalentProbability = 0.846134590697761
weightedProbabilityScore = 0.363960173151333
equivalentWeightedProbability = 0.929703311206423

results (on 2014-6-13) with new data
typical longtermPrediction error = forgot to check
typicalScoreError = 0.17827281478312368
typicalProbabilityError = 0.35591011031817193
equivalentProbability = 0.85118085564749491
weightedProbabilityScore = 0.38300734457703611
equivalentWeightedProbability = 0.93253643708711076

results (on 2014-06-14) with a slightly different algorithm in hopes of faster runtime, which was then reverted
typical longtermPrediction error = 0.137385512266307
typicalScoreError = 0.178869611844413
typicalProbabilityError = 0.357426209239901
equivalentProbability = 0.849637676671714
weightedProbabilityScore = 0.369311618622313
equivalentWeightedProbability = 0.930512129717936

results (on 2014-08-17) after making the activity progressions lazy but without yet incorporating data newer than 2014-06-08
typical longtermPrediction error = 0.137357092708936
typicalScoreError = 0.178515659914813
typicalProbabilityError = 0.369035683053002
equivalentProbability = 0.837361326523365
weightedProbabilityScore = 0.316766255842736
equivalentWeightedProbability = 0.922108643100204


results (on 2014-08-24) after splitting boxes (more quickly) using a better approximation of the median than just the middle of the bounding box, but without yet incorporating new data
typical longtermPrediction error = 0.137054692228206
typicalScoreError = 0.183816909567503
typicalProbabilityError = 0.380240802659689
equivalentProbability = 0.824679737576454
weightedProbabilityScore = 0.2146129963213
equivalentWeightedProbability = 0.902361777364356


results (on 2015-03-07) after getting more data (due to the passage of time)
typical longtermPrediction error = 0.133384666651803
typicalScoreError = 0.182199677921326
typicalProbabilityError = 0.389949448971249
equivalentProbability = 0.812952755614996
weightedProbabilityScore = 0.154158837166051
equivalentWeightedProbability = 0.888035588760429
 
 
results (on 2015-03-07) after making the AdaptiveInterpolator initially do some blind splits of some dimensions for a lot more startup speed and a little less complexity
typical longtermPrediction error = 0.133722875018643
typicalScoreError = 0.181529779100519
typicalProbabilityError = 0.387463782531814
equivalentProbability = 0.816025026265546
weightedProbabilityScore = 0.181927056060576
equivalentWeightedProbability = 0.894896166812173

results (on 2015-03-07) after new data (plus ignoring any parent that already has lots of considerations on startup)
typical longtermPrediction error = 0.133944756855872
typicalScoreError = 0.178996813609837
typicalProbabilityError = 0.393833200208718
equivalentProbability = 0.808050986710577
weightedProbabilityScore = 0.085217612980164
equivalentWeightedProbability = 0.868608281702737


results (on 2015-10-03) after new data
typical longtermPrediction error = 0.129196623418494
typicalScoreError = 0.180094412105045
typicalProbabilityError = 0.391209982636509
equivalentProbability = 0.811375576250839
weightedProbabilityScore = 0.0721504114768844
equivalentWeightedProbability = 0.864486053300514


results (on 2015-10-18) after new data and some refactoring
typical longtermPrediction error = 0.132303474241546
typicalScoreError = 0.179842980952335
typicalProbabilityError = 0.392650963144817
equivalentProbability = 0.809556491034266
weightedProbabilityScore = 0.0968530252359711
equivalentWeightedProbability = 0.872152190471373

same results, same day, after fixing the dates of the skips

updated results (still on 2015-10-18) after using the RatingRenormalizer to recompute the absolute portion of each RelativeRating
 (note that this ignores the previous absolute ratings that originally weren't ignored and starts the ratings at 0.5)
typical longtermPrediction error = 0.0306978007961018
typicalScoreError = 0.119055754172878
typicalProbabilityError = 0.392650963144817
equivalentProbability = 0.809556491034266
weightedProbabilityScore = 0.0968530252359711
equivalentWeightedProbability = 0.872152190471373
 
 
results (on 2017-08-05) after getting more data (due to the passage of time)
typical longtermPrediction error = 0.11623802086738
typicalScoreError = 0.129285744904529
typicalProbabilityError = 0.340633910429981
equivalentProbability = 0.866017129469619
weightedProbabilityScore = 0.293795953009491
equivalentWeightedProbability = 0.918089807237037


results on 2017-08-13 (using data through 2017-08-05) after having modified the algorithm
The algorithm compares the cumulative time spent against its least-squares-regression-line, for predicting ratings and participation probabilities
typical longtermPrediction error = 0.104498926030268
typicalScoreError = 0.133200373078405
typicalProbabilityError = 0.342376710582286
equivalentProbability = 0.864387414781101
weightedProbabilityScore = 0.31189500109831
equivalentWeightedProbability = 0.921274895962403

updated results on 2017-08-13 (with new data)
typical longtermPrediction error = 0.104718699486009
typicalScoreError = 0.133140687667085
typicalProbabilityError = 0.342444414621633
equivalentProbability = 0.864323788537679
weightedProbabilityScore = 0.311459030520166
equivalentWeightedProbability = 0.921199799038855
 
updated results on 2018-02-04 with new data
typical longtermPrediction error = 0.0995526585842484
typicalScoreError = 0.142199611173665
typicalProbabilityError = 0.342892978994635
equivalentProbability = 0.863901641870691
weightedProbabilityScore = 0.293463192543033
equivalentWeightedProbability = 0.918029930119905

updated results on 2018-02-19 with new data
typical longtermPrediction error = 0.10743172314635
typicalScoreError = 0.138030913442599
typicalProbabilityError = 0.343135756731594
equivalentProbability = 0.86367272712184
weightedProbabilityScore = 0.287756896146186
equivalentWeightedProbability = 0.916995556809255

updated results on 2018-02-19 when using estimated ratings to help compute longterm value
typical longtermPrediction error = 0.0938182895594615
typicalScoreError = 0.138030913442599
typicalProbabilityError = 0.343135756731594
equivalentProbability = 0.86367272712184
weightedProbabilityScore = 0.287756896146186
equivalentWeightedProbability = 0.916995556809255

updated results on 2018-02-19 after some fixes
typical longtermPrediction error = 0.0891772964258622
typicalScoreError = 0.138030913442599
typicalProbabilityError = 0.343135756731594
equivalentProbability = 0.86367272712184
weightedProbabilityScore = 0.287756896146186
equivalentWeightedProbability = 0.916995556809255

updated results on 2018-06-17 with new data
typical longtermPrediction error = 0.0833641928173899
typicalScoreError = 0.137588255915862
typicalProbabilityError = 0.342836995815694
equivalentProbability = 0.863954384916668
weightedProbabilityScore = 0.288373617715065
equivalentWeightedProbability = 0.917108043178257

updated results on 2018-06-17 after having tuned some constants
typical longtermPrediction error = 0.0796780520090059
typicalScoreError = 0.137588255915862
typicalProbabilityError = 0.342836995815694
equivalentProbability = 0.863954384916668
weightedProbabilityScore = 0.288373617715065
equivalentWeightedProbability = 0.917108043178257

updated results on 2018-06-19 after getting a little bit of new data and adding longtermPredictionIfParticipated
typical longtermPredictionIfSuggested error = 0.0803238577232935
typical longtermPredictionIfParticipated error = 0.232883937714255
typicalScoreError = 0.137588255915862
typicalProbabilityError = 0.342831553588421
equivalentProbability = 0.863959511297823
weightedProbabilityScore = 0.288292895034349
equivalentWeightedProbability = 0.917093329453631

updated results on 2018-06-19 after improving the accuracy of Engine.Get_Overall_ParticipationEstimate
typical longtermPredictionIfSuggested error = 0.0803238577232935
typical longtermPredictionIfParticipated error = 0.072638658082502
typicalScoreError = 0.137588255915862
typicalProbabilityError = 0.342831553588421
equivalentProbability = 0.863959511297823
weightedProbabilityScore = 0.288292895034349
equivalentWeightedProbability = 0.917093329453631

updated results on 2018-07-29 after getting some new data
typical longtermPredictionIfSuggested error = 0.0769317230966445
typical longtermPredictionIfParticipated error = 0.0687857499511654
typicalScoreError = 0.137300194760722
typicalProbabilityError = 0.342963751469292
equivalentProbability = 0.863834942217085
weightedProbabilityScore = 0.285088970267528
equivalentWeightedProbability = 0.916506989305579

updated results on 2018-07-29 after some fixups discovered when switching to EstimateSuggestionValue instead of MakeRecommendation
typical longtermPredictionIfSuggested error = 0.0776676670385656
typical longtermPredictionIfParticipated error = 0.0677669750654978
typicalScoreError = 0.136244463813144
typicalProbabilityError = 0.337362033552108
equivalentProbability = 0.869035036707338
weightedProbabilityScore = 0.287448503981239
equivalentWeightedProbability = 0.916939244447084

updated results again on 2018-07-29 after a tiny bit more data, plus some tuning of Get_Overall_SuggestionEstimate to favor parent activities more highly
typical longtermPredictionIfSuggested error = 0.072384304970362
typical longtermPredictionIfParticipated error = 0.067750639659817
typicalScoreError = 0.136360253685622
typicalProbabilityError = 0.3373837026757
equivalentProbability = 0.869015226201894
weightedProbabilityScore = 0.290649609017511
equivalentWeightedProbability = 0.917521710819241

updated results on 2018-08-12 with new data
typical longtermPredictionIfSuggested error = 0.068462838080553
typical longtermPredictionIfParticipated error = 0.0640969948485463
typicalScoreError = 0.13508617311407
typicalProbabilityError = 0.337732398701453
equivalentProbability = 0.868696117239337
weightedProbabilityScore = 0.288184129377369
equivalentWeightedProbability = 0.917073499618037

updated results on 2018-08-12 after some tuning: having AdaptiveInterpolator consider more points for splitting,
and using TimeProgression.AbsoluteTime as a factor for predicting activity ratings
typical longtermPredictionIfSuggested error = 0.0660279601785022
typical longtermPredictionIfParticipated error = 0.0455410485831656
typicalScoreError = 0.140825893664029
typicalProbabilityError = 0.3396555229262
equivalentProbability = 0.866925231819413
weightedProbabilityScore = 0.307009231292974
equivalentWeightedProbability = 0.920428780086327
EngineTester completed in 00:03:00.5293930

updated results on 2018-02-27 with more data
Also, this now runs outside of the debugger so I didn't copy-paste the entire numbers for this run
typical longtermPredictionIfSuggestedError = 0.0666
typical longtermPredictionIfParticipated error = 0.0443
typicalScoreError = 0.1359
equivalentWeightedProbability = 0.9201
completed in 00:01:31.8

updated results on 2018-09-22 with more data
Reminder that this runs outside the debugger so I didn't bother copy-pasting the entire numbers of this run either
typical longtermPredictionIfSuggested = 0.0723
typical longtermPredictionIfParticipated = 0.0439
typicalScoreError = 0.1331
equivalentWeightedProbability = 0.9207

updated results on 2018-09-22 after fixing longtermValue predictions to still incorporate skip probability when skip probability is high
typical longtermPredictionIfSuggested = 0.0722
typical longtermPredictionIfParticipated = 0.0439
typicalScoreError = 0.1331
equivalentWeightedProbability = 0.9207

updated results on 2018-09-23 with more data
0.0681 typical longtermPredictionIfSuggested
0.0439 typical longtermPredictionIfParticipated
0.1329 typicalScoreError
0.9204 equivalentWeightedProbability

updated results on 2018-09-23 after making the longtermValue predictions react to new data more quickly
0.0683 typical longtermPredictionIfSuggested
0.0421 typical longtermPredictionIfParticipated
0.1329 typicalScoreError
0.9204 equivalentWeightedProbability

updated results on 2018-09-30 with more data
0.0785 typical longtermPredictionIfSuggested error
0.0422 typical longtermPredictionIfParticipated error
0.1326 typicalScoreError
0.9237 equivalentWeightedProbability

updated results on 2018-09-30 after making the longtermValue predictions be less overpowering
Unfortunately this change slightly increases the error rate, even after making some adjustments to lower the error rate again.
However, this prevents the experiment screen from appearing to get stuck on the same suggestion repeatedly.
0.0788 typical longtermPredictionIfSuggested
0.0422 typical longtermPredictionIfParticipated
0.1326 typicalScoreError
0.9237 equivalentWeightedProbability

updated results on 2018-10-13 with new data
0.0851 typical longtermPredictionIfSuggested error
0.0427 typical longtermPredictionIfParticipated error
0.1315 typicalScoreError
0.9290 equivalentWeightedProbability

updated results on 2018-10-13 after making the the suggestions update to new data more quickly
0.0779 longtermPredictionIfSuggested error
0.0465 longtermPredictionIfParticipated error
0.1317 typicalScoreError
0.9249 equivalentWeightedProbability

updated results on 2018-10-14 with new data
0.0786 typical longtermPredictionIfSuggested error
0.0465 typical longtermPredictionIfParticipated error
0.1313 typicalScoreError
0.9298 equivalentWeightedProbability

updated results on 2018-10-14 after improving the participation probability estimates
0.0792 typical longtermPredictionIfSuggested error
0.0460 typical longtermPredictionIfParticipated error
0.1324 typicalScoreError
0.9349 equivalentWeightedProbability

updated results on 2018-11-03 with new data, plus also computing longtermEfficiencyPredictions error
0.0771747950148125 typical longtermPredictionIfSuggested error
0.0446777201816527 typical longtermPredictionIfParticipated error
0.131600649875188 typicalScoreError
0.935157989467304 equivalentWeightedProbability
1.16259387446236 typicalEfficiencyError
0.149482265649881 typical longtermEfficiencyIfParticipated error

updated results on 2018-11-04 after what was supposed to be a refactor but ended up introducing small changes too
0.0772857169301227 typical longtermPredictionIfSuggested error
0.0447131366293045 typical longtermPredictionIfParticipated error
0.131708768952102 typicalScoreError
0.935149685404044 equivalentWeightedProbability
1.16259387446236 typicalEfficiencyError
0.149467844999278 typical longtermEfficiencyIfParticipated error

updated results on 2018-11-04 after a little bit of tuning to make up for the previous slight decrease in accuracy
0.0772857169301227 typical longtermPredictionIfSuggested error
0.0440196026228229 typical longtermPredictionIfParticipated error
0.131708768952102 typicalScoreError
0.935149685404044 equivalentWeightedProbability
1.16259387446236 typicalEfficiencyError
0.149467844999278 typical longtermEfficiencyIfParticipated error

updated results on 2018-12-09 with new data
0.0852 typical longtermPredictionIfSuggested error
0.0425 typical longtermPredictionIfParticipated error
0.1270 typicalScoreError
0.9370 equivalentWeightedProbability
2.8188 typicalEfficiencyError
0.1982 typical longtermEfficiencyIfParticipated error
Computed results in 00:02:40

updated results on 2018-12-09 after some tweaks to AdaptiveLinearInterpolator to facilitate making findRepresentativePoints terminate
0.0853 typical longtermPredictionIfSuggested error
0.0425 typical longtermPredictionIfParticipated error
0.1271 typicalScoreError
0.9369 equivalentWeightedProbability
2.8188 typicalEfficiencyError
0.1982 typical longtermEfficiencyIfParticipated error
Computed results in 00:02:40

updated results on 2018-12-13 after a small amount of new data, plus some bugfixes to make multidimensional interpolation work better
0.0783 typical longtermPredictionIfSuggested error
0.0434 typical longtermPredictionIfParticipated error
0.1312 typicalScoreError
0.9350 equivalentWeightedProbability
2.8054 typicalEfficiencyError
0.1996 typical longtermEfficiencyIfParticipated error
Computed results in 00:02:41

updated results on 2019-01-14 after some new data
0.0827 typical longtermPredictionIfSuggested error
0.0423 typical longtermPredictionIfParticipated error
0.1288 typicalScoreError
0.9438 equivalentWeightedProbability
3.8972 typicalEfficiencyError
0.5102 typical longtermEfficiencyIfParticipated error

updated results on 2019-01-14 after making ratingTestingProgressions match ratingTrainingProgressions
0.0828 typical longtermPredictionIfSuggested error
0.0424 typical longtermPredictionIfParticipated error
0.1279 typicalScoreError
0.9438 equivalentWeightedProbability
3.8972 typicalEfficiencyError
0.5102 typical longtermEfficiencyIfParticipated error
Computed results in 00:03:02

updated results on 2019-03-10 with new data
0.0804 typical longtermPredictionIfSuggested error
0.0420 typical longtermPredictionIfParticipated error
0.1252 typicalScoreError
0.9491 equivalentWeightedProbability
4.3317 typicalEfficiencyError
0.6912 typical longtermEfficiencyIfParticipated error
Computed results in 00:04:15

updated results on 2019-03-10 after adding 1 second to ActivitySkip.ThinkingTime
0.0997 typical longtermPredictionIfSuggested error
0.0420 typical longtermPredictionIfParticipated error
0.1252 typicalScoreError
0.9491 equivalentWeightedProbability
4.3317 typicalEfficiencyError
0.6912 typical longtermEfficiencyIfParticipated error

updated results on 2019-11-29 with new data:
0.1044 typical longtermPredictionIfSuggested error
0.0289 typical longtermPredictionIfParticipated error
0.1277 typicalScoreError
0.9604 equivalentWeightedProbability
3.8010 typicalEfficiencyError =
0.9207 typical longtermEfficiencyIfParticipated error
Computed results in 00:05:09

Updated results after ignoring predictions that are earlier in time than the first point in time known to the corresponding ScoreSummarizer:
(Note that I have several more years of participation and rating data than efficiency data, making it not that interesting to repeatedly use lots of data from years ago to predict what my efficiency was going to become once I started measuring it)
0.0912 typical longtermPredictionIfSuggested error
0.0289 typical longtermPredictionIfParticipated error
0.1277 typicalScoreError
0.9604 equivalentWeightedProbability
3.8010 typicalEfficiencyError
1.8448 typical longtermEfficiencyIfParticipated error

updated results on 2020-02-01 with more data:
0.0900 typical longtermPredictionIfSuggested error
0.0267 typical longtermPredictionIfParticipated error
0.1321 typicalScoreError
0.9605 equivalentWeightedProbability
3.6634 typicalEfficiencyError
1.7822 typical longtermEfficiencyIfParticipated error
EngineTester completed in 00:08:32.9351070 // when running in the debugger

updated results on 2020-07-11 with more data:
0.0889 typical longtermPredictionIfSuggested error
0.0269 typical longtermPredictionIfParticipated error
0.1351 typicalScoreError
NaN    equivalentWeightedProbability
3.6981 typicalEfficiencyError
1.7382 typical longtermEfficiencyIfParticipated error
EngineTester completed in 00:02:07.1717935 // when running in the debugger on a laptop

updated results after predicting longterm happiness from longterm efficiency
0.0886 typical longtermPredictionIfSuggested error
0.0263 typical longtermPredictionIfParticipated error
0.1351 typicalScoreError
NaN    equivalentWeightedProbability
3.6981 typicalEfficiencyError
1.7414 typical longtermEfficiencyIfParticipated error
EngineTester completed in 00:02:07.8680408 // when running in the debugger on a laptop

updated results after fixing some incorrect participation intensities
0.0901 typical longtermPredictionIfSuggested error
0.0263 typical longtermPredictionIfParticipated error
0.1351 typicalScoreError
0.9592 equivalentWeightedProbability
3.6981 typicalEfficiencyError
1.7414 typical longtermEfficiencyIfParticipated error
EngineTester completed in 00:02:07.7771503

updated results on 2021-01-18 after getting new data, plus adjusting some weights to make the engine less likely to repeat itself
0.0551 typical longtermPredictionIfSuggested error
0.0242 typical longtermPredictionIfParticipated error
0.1403 typicalScoreError
0.9609 equivalentWeightedProbability
3.6219 typicalEfficiencyError
1.7385 typical longtermEfficiencyIfParticipated error
EngineTester completed in 00:02:09.1026894 // when running in the debugger on a laptop

updated results after changing the format and also calculating the average of the errors of the standard deviations estimated for the predictions
Means.MeanErr: 0.0551, StdDevs.MeanErr: 0.2218, longtermHappinessIfSuggested
Means.MeanErr: 0.0242, StdDevs.MeanErr: 0.1717, longtermHappinessIfParticipated
Means.MeanErr: 0.1403, StdDevs.MeanErr: 0.1315, score
0.9609,                                         equivalentWeightedProbability
Means.MeanErr: 3.6219, StdDevs.MeanErr: 3.4153, efficiency
Means.MeanErr: 1.7385, StdDevs.MeanErr: 1.5702, longtermEfficiencyIfParticipated
EngineTester completed in 00:02:09.9540600

updated results after adjusting some weights, especially for tuning standard deviations
Means.MeanErr: 0.0476, StdDevs.MeanErr: 0.0946, longtermHappinessIfSuggested
Means.MeanErr: 0.0240, StdDevs.MeanErr: 0.0954, longtermHappinessIfParticipated
Means.MeanErr: 0.1403, StdDevs.MeanErr: 0.1040, score
0.9609,                                         equivalentWeightedProbability
Means.MeanErr: 3.6219, StdDevs.MeanErr: 3.4153, efficiency
Means.MeanErr: 1.7385, StdDevs.MeanErr: 1.5702, longtermEfficiencyIfParticipated
EngineTester completed in 00:02:10.3737859

updated results on 2021-04-18 after getting new data and some fixes to efficiency calculations
Means.MeanErr: 0.0474, StdDevs.MeanErr: 0.0945, longtermHappinessIfSuggested
Means.MeanErr: 0.0240, StdDevs.MeanErr: 0.0953, longtermHappinessIfParticipated
Means.MeanErr: 0.1406, StdDevs.MeanErr: 0.1043, score
0.9618,                                         equivalentWeightedProbability
Means.MeanErr: 5.4510, StdDevs.MeanErr: 5.7184, efficiency
Means.MeanErr: 0.3961, StdDevs.MeanErr: 0.7620, longtermEfficiencyIfParticipated
EngineTester completed in 00:02:04.1933960

updated results on 2021-04-18 after making the happiness-if-suggested estimates ignore participation predictions for activities the user has never done
Means.MeanErr: 0.0444, StdDevs.MeanErr: 0.0931, longtermHappinessIfSuggested
Means.MeanErr: 0.0240, StdDevs.MeanErr: 0.0952, longtermHappinessIfParticipated
Means.MeanErr: 0.1406, StdDevs.MeanErr: 0.1042, score
0.9618,                                         equivalentWeightedProbability
Means.MeanErr: 5.4510, StdDevs.MeanErr: 5.7184, efficiency
Means.MeanErr: 0.3963, StdDevs.MeanErr: 0.7646, longtermEfficiencyIfParticipated
EngineTester completed in 00:02:04.1933960

updated results on 2021-04-18 after fixing the happiness-if-suggested estimates to better combine the participation components of happiness suggestion estimates (like errors rather than like weights)
Means.MeanErr: 0.0298, StdDevs.MeanErr: 0.095, longtermHappinessIfSuggested
Means.MeanErr: 0.0240, StdDevs.MeanErr: 0.0952, longtermHappinessIfParticipated
Means.MeanErr: 0.1406, StdDevs.MeanErr: 0.1042, score
0.9618,                                         equivalentWeightedProbability
Means.MeanErr: 5.4510, StdDevs.MeanErr: 5.7184, efficiency
Means.MeanErr: 0.3963, StdDevs.MeanErr: 0.7646, longtermEfficiencyIfParticipated
EngineTester completed in 00:02:04.1933960

updated results on 2021-04-24 after switching to LazyDimension_Interpolator but before adding more coordinates to it
Means.MeanErr: 0.0313, StdDevs.MeanErr: 0.0951, longtermHappinessIfSuggested
Means.MeanErr: 0.0244, StdDevs.MeanErr: 0.0949, longtermHappinessIfParticipated
Means.MeanErr: 0.1406, StdDevs.MeanErr: 0.1042, score
0.9619,                                         equivalentWeightedProbability
Means.MeanErr: 5.451, StdDevs.MeanErr: 5.7184, efficiency
Means.MeanErr: 0.3933, StdDevs.MeanErr: 0.8445, longtermEfficiencyIfParticipated
EngineTester completed in 00:02:09.7762550

some test results on 2021-04-24 when trying out what happens if no input coordinates are passed to the longterm value interpolators
Means.MeanErr: 0.034, StdDevs.MeanErr: 0.0791, longtermHappinessIfSuggested
Means.MeanErr: 0.0261, StdDevs.MeanErr: 0.1044, longtermHappinessIfParticipated
Means.MeanErr: 0.1406, StdDevs.MeanErr: 0.1042, score
0.9635,                                         equivalentWeightedProbability
Means.MeanErr: 5.451, StdDevs.MeanErr: 5.7184, efficiency
Means.MeanErr: 1.142, StdDevs.MeanErr: 1.142, longtermEfficiencyIfParticipated

updated results on 2021-04-25 after passing lots of coordinates to the LazyDimension_Interpolator
Means.MeanErr: 0.0266, StdDevs.MeanErr: 0.033, longtermHappinessIfSuggested
Means.MeanErr: 0.025, StdDevs.MeanErr: 0.0671, longtermHappinessIfParticipated
Means.MeanErr: 0.1394, StdDevs.MeanErr: 0.1058, score
0.9622,                                         equivalentWeightedProbability
Means.MeanErr: 5.4443, StdDevs.MeanErr: 5.7141, efficiency
Means.MeanErr: 0.4013, StdDevs.MeanErr: 0.564, longtermEfficiencyIfParticipated
EngineTester completed in 00:01:51

updated results on 2021-05-08 after adding another output format
Means.MeanErr: 0.0266 (19 days), StdDevs.MeanErr: 0.033 (24 days), longtermHappinessIfSuggested
Means.MeanErr: 0.025 (18 days), StdDevs.MeanErr: 0.0671 (49 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1394 (0.2626 * average), StdDevs.MeanErr: 0.1058 (0.1993 * average), score
0.9622,                                         equivalentWeightedProbability
Means.MeanErr: 5.4443 (3.0786 * average), StdDevs.MeanErr: 5.7141 (3.2311 * average), efficiency
Means.MeanErr: 0.4013 (3 days), StdDevs.MeanErr: 0.564 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:00.9650314

updated results on 2021-07-04 with more data
Means.MeanErr: 0.0278 (20 days), StdDevs.MeanErr: 0.0338 (25 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0251 (18 days), StdDevs.MeanErr: 0.0669 (49 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1406 (0.2648 * average), StdDevs.MeanErr: 0.1059 (0.1995 * average), score
0.9614,                                         equivalentWeightedProbability
Means.MeanErr: 5.2399 (3.0475 * average), StdDevs.MeanErr: 5.4989 (3.1982 * average), efficiency
Means.MeanErr: 0.4013 (3 days), StdDevs.MeanErr: 0.564 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:04.1935225

updated results on 2021-07-05 after some correctness fixes: now incorporating the future values of non-suggested participations, and also having the LongtermValuePredictor update more often
and some accuracy improvements ScoreSummary incorporating weight, new interpolator that considers weight when splitting, and child activities having few participations incorporating predictions from parents having lots of participations
Means.MeanErr: 0.0325 (24 days), StdDevs.MeanErr: 0.0283 (21 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0321 (23 days), StdDevs.MeanErr: 0.03 (22 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1395 (0.2628 * average), StdDevs.MeanErr: 0.1039 (0.1957 * average), score
0.9471,                                         equivalentWeightedProbability
Means.MeanErr: 5.2664 (3.0629 * average), StdDevs.MeanErr: 5.6189 (3.2679 * average), efficiency
Means.MeanErr: 0.3916 (3 days), StdDevs.MeanErr: 0.3389 (2 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:39.5356054

updated results on 2021-07-06 with new data:
Means.MeanErr: 0.0438 (32 days), StdDevs.MeanErr: 0.0398 (29 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0437 (32 days), StdDevs.MeanErr: 0.0411 (30 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1394 (0.2626 * average), StdDevs.MeanErr: 0.1039 (0.1957 * average), score
0.9472,                                         equivalentWeightedProbability
Means.MeanErr: 5.2333 (3.0646 * average), StdDevs.MeanErr: 5.584 (3.27 * average), efficiency
Means.MeanErr: 0.6508 (5 days), StdDevs.MeanErr: 0.5815 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:29.8838670

updated results on 2021-07-06 when exluding the last week of predictions from error calculations:
Means.MeanErr: 0.0323 (24 days), StdDevs.MeanErr: 0.0281 (20 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0319 (23 days), StdDevs.MeanErr: 0.0298 (22 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1394 (0.2626 * average), StdDevs.MeanErr: 0.1039 (0.1957 * average), score
0.9472,                                         equivalentWeightedProbability
Means.MeanErr: 5.2333 (3.0646 * average), StdDevs.MeanErr: 5.584 (3.27 * average), efficiency
Means.MeanErr: 0.6512 (5 days), StdDevs.MeanErr: 0.5824 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:31.2458927

updated results on 2021-07-06 when making suggestion estimate more closely match participation estimate
Means.MeanErr: 0.0328 (24 days), StdDevs.MeanErr: 0.0274 (20 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0319 (23 days), StdDevs.MeanErr: 0.0298 (22 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1394 (0.2626 * average), StdDevs.MeanErr: 0.1039 (0.1957 * average), score
0.9472,                                         equivalentWeightedProbability
Means.MeanErr: 5.2333 (3.0646 * average), StdDevs.MeanErr: 5.584 (3.27 * average), efficiency
Means.MeanErr: 0.6512 (5 days), StdDevs.MeanErr: 0.5824 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:29.8333621

updated results on 2021-07-11 after interpolating based on how long ago the user considered specifically this activity rather than each activity
Means.MeanErr: 0.0328 (24 days), StdDevs.MeanErr: 0.0273 (20 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0316 (23 days), StdDevs.MeanErr: 0.0299 (22 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1394 (0.2626 * average), StdDevs.MeanErr: 0.1039 (0.1957 * average), score
0.9472,                                         equivalentWeightedProbability
Means.MeanErr: 5.2333 (3.0646 * average), StdDevs.MeanErr: 5.584 (3.27 * average), efficiency
Means.MeanErr: 0.6524 (5 days), StdDevs.MeanErr: 0.5704 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:28.7317889

updated results on 2021-07-25 after including participation relative ratings that explicitly specify both participation names
Means.MeanErr: 0.029 (21 days), StdDevs.MeanErr: 0.0246 (18 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0277 (20 days), StdDevs.MeanErr: 0.0264 (19 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1395 (0.2629 * average), StdDevs.MeanErr: 0.104 (0.1958 * average), score
0.9472,                                         equivalentWeightedProbability
Means.MeanErr: 5.2333 (3.0646 * average), StdDevs.MeanErr: 5.584 (3.27 * average), efficiency
Means.MeanErr: 0.6525 (5 days), StdDevs.MeanErr: 0.5704 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:03:02.5331696

updated results on 2021-07-25 with new data
Means.MeanErr: 0.028 (20 days), StdDevs.MeanErr: 0.0244 (18 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0284 (21 days), StdDevs.MeanErr: 0.0263 (19 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1398 (0.2634 * average), StdDevs.MeanErr: 0.104 (0.196 * average), score
0.9477,                                         equivalentWeightedProbability
Means.MeanErr: 5.2333 (3.0646 * average), StdDevs.MeanErr: 5.584 (3.27 * average), efficiency
Means.MeanErr: 0.6526 (5 days), StdDevs.MeanErr: 0.5697 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:57.9593696

updated results on 2021-07-25 after switching from a happinessIfSuggested interpolator to a happinessIfSkipped interpolator
Means.MeanErr: 0.0282 (21 days), StdDevs.MeanErr: 0.0242 (18 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0284 (21 days), StdDevs.MeanErr: 0.0263 (19 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1398 (0.2634 * average), StdDevs.MeanErr: 0.104 (0.196 * average), score
0.9477,                                         equivalentWeightedProbability
Means.MeanErr: 5.2333 (3.0646 * average), StdDevs.MeanErr: 5.584 (3.27 * average), efficiency
Means.MeanErr: 0.6526 (5 days), StdDevs.MeanErr: 0.5697 (4 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:02:15.5670875

updated results on 2022-05-15 with new data
Means.MeanErr: 0.0314 (23 days), StdDevs.MeanErr: 0.0248 (18 days), longtermHappinessIfSuggested
Means.MeanErr: 0.0277 (20 days), StdDevs.MeanErr: 0.0258 (19 days), longtermHappinessIfParticipated
Means.MeanErr: 0.1414 (0.2662 * average), StdDevs.MeanErr: 0.1048 (0.1972 * average), score
0.9461,                                         equivalentWeightedProbability
Means.MeanErr: 5.2198 (3.0362 * average), StdDevs.MeanErr: 5.569 (3.2394 * average), efficiency
Means.MeanErr: 3.678 (26 days), StdDevs.MeanErr: 3.5606 (25 days), longtermEfficiencyIfParticipated
EngineTester completed in 00:03:00.0605745 // running in the debugger
*/

namespace ActivityRecommendation
{
    class EngineTester : HistoryReplayer
    {
        public EngineTester()
        {
            this.ratingSummarizer = new ExponentialRatingSummarizer(CommonPreferences.Instance.HalfLife);
            this.ratingSummarizer.Description = "EngineTester Score Summarizer";
            this.efficiencySummarizer = new ExponentialRatingSummarizer(CommonPreferences.Instance.EfficiencyHalflife);
            this.efficiencySummarizer.Description = "EngineTester Efficiency Summarizer";
            this.executionStart = DateTime.Now;
        }
        public override AbsoluteRating ProcessRating(AbsoluteRating newRating)
        {
            // ignore generated ratings
            if (!newRating.FromUser)
                return newRating;
            if (this.EarliestRatingDate == null)
                this.EarliestRatingDate = newRating.Date;

            if (this.numRatings % 1000 == 0)
            {
                this.PrintResults();
                System.Diagnostics.Debug.WriteLine("Adding rating with date " + ((DateTime)newRating.Date).ToString());
            }
            this.numRatings++;


            this.UpdateScoreError(newRating, (DateTime)newRating.Date, newRating.Score);
            RatingSource ratingSource = newRating.Source;
            // the code that figures out where a rating came from only checks the most-recently-entered participation
            // Occasionally this participation doesn't match (and we don't yet bother scanning further back), so it's posible that the rating source can be null
            if (ratingSource != null)
            {
                Participation sourceParticipation = ratingSource.ConvertedAsParticipation;
                if (sourceParticipation != null)
                {
                    this.ratingSummarizer.AddRating(sourceParticipation.StartDate, sourceParticipation.EndDate, newRating.Score);
                }
            }
            return newRating;
        }

        public override void PreviewParticipation(Participation newParticipation)
        {
            this.updatePredictionsIfParticipated(newParticipation.ActivityDescriptor, newParticipation.StartDate);

            // update the error rate for the participation probability predictor
            if (newParticipation.Suggested)
            {
                // if the activity was certaintly not suggested, then we don't want to include it in our estimate 
                // of "the probability that the user would do the activity, given that it was suggested"
                this.UpdateParticipationProbabilityError(newParticipation.ActivityDescriptor, newParticipation.StartDate, 1);
            }


            this.ratingSummarizer.AddParticipationIntensity(newParticipation.StartDate, newParticipation.EndDate, 1);
            if (newParticipation.RelativeEfficiencyMeasurement != null)
            {
                RelativeEfficiencyMeasurement laterMeasurement = newParticipation.RelativeEfficiencyMeasurement;
                RelativeEfficiencyMeasurement earlierMeasurement = laterMeasurement.Earlier;
                this.addEfficiencyMeasurement(earlierMeasurement);
                this.addEfficiencyMeasurement(laterMeasurement);
            }
        }

        private void addEfficiencyMeasurement(RelativeEfficiencyMeasurement measurement)
        {
            Distribution computedEfficiency = measurement.RecomputedEfficiency;
            this.efficiencySummarizer.AddRating(measurement.StartDate, measurement.EndDate, computedEfficiency.Mean);
            this.Update_ShortTermEfficiency_Error(measurement.ActivityDescriptor, measurement.StartDate, measurement.RecomputedEfficiency.Mean);
        }

        public override void PreviewSkip(ActivitySkip newSkip)
        {
            // update the error rate for the participation probability predictor
            foreach (ActivityDescriptor descriptor in newSkip.ActivityDescriptors)
            {
                this.UpdateParticipationProbabilityError(descriptor, newSkip.CreationDate, 0);
            }
            // inform the ratingSummarizer that the user wasn't doing anything during this time
            this.ratingSummarizer.AddParticipationIntensity(newSkip.ConsideredSinceDate, newSkip.ThinkingTime, 0);
        }

        // runs the engine on the given activity at the given date, and keeps track of the overall error
        public void UpdateScoreError(AbsoluteRating rating, DateTime when, double correctScore)
        {
            ActivityRequest request = new ActivityRequest(when);
            // compute estimated score
            Activity activity = this.activityDatabase.ResolveDescriptor(rating.ActivityDescriptor);
            this.engine.EstimateFutureHappinessIfSuggested(activity, request);
            Prediction prediction = this.engine.EstimateRating(activity, when);
            double expectedRating = prediction.Distribution.Mean;

            // compute error
            double error = correctScore - expectedRating;
            this.shortTermScore_error.Add(when, correctScore, prediction.Distribution);

            // update most surprising participation
            double numSecondsSinceFirstRating = when.Subtract(this.EarliestRatingDate.Value).TotalSeconds;
            double totalSurprise = Math.Abs(error * numSecondsSinceFirstRating);
            if (this.mostSurprisingParticipation == null || totalSurprise > this.mostSurprisingParticipation.Surprise)
            {
                ParticipationSurprise newSurprise = new ParticipationSurprise();
                newSurprise.Surprise = totalSurprise;
                newSurprise.ExpectedRating = expectedRating;
                newSurprise.ActualRating = correctScore;
                newSurprise.Date = when;
                newSurprise.ActivityDescriptor = rating.ActivityDescriptor;

                this.mostSurprisingParticipation = newSurprise;
            }
        }
        public void updatePredictionsIfParticipated(ActivityDescriptor descriptor, DateTime when)
        {
            ActivityRequest request = new ActivityRequest(when);
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            Prediction predictionForSuggestion = this.engine.EstimateFutureHappinessIfSuggested(activity, request);

            if (predictionForSuggestion.Distribution.Weight > 0)
            {
                ScoreSummary ratingSummary = new ScoreSummary(when);
                this.valueIfSuggested_predictions[predictionForSuggestion] = ratingSummary;

                Prediction predictionIfParticipated = this.engine.Get_OverallHappiness_ParticipationEstimate(activity, request);
                this.valueIfParticipated_predictions[predictionIfParticipated] = ratingSummary;

                ScoreSummary efficiencySummary = new ScoreSummary(when);
                Prediction efficiencyIfParticipated = this.engine.Get_OverallEfficiency_ParticipationEstimate(activity, when);
                this.efficiencyIfParticipated_predictions[efficiencyIfParticipated] = efficiencySummary;
            }

        }
        public void UpdateParticipationProbabilityError(ActivityDescriptor descriptor, DateTime when, double actualIntensity)
        {
            if (actualIntensity > 1 || actualIntensity < 0)
                System.Diagnostics.Debug.WriteLine("Invalid participation intensity: " + actualIntensity);
            // compute the estimate participation probability
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);

            double predictedProbability = this.engine.EstimateParticipationProbability(activity, when).Distribution.Mean;
            double error = predictedProbability - actualIntensity;
            Distribution errorDistribution = Distribution.MakeDistribution(error * error, 0, 1);
            this.squaredParticipationProbabilityError = this.squaredParticipationProbabilityError.Plus(errorDistribution);
            // We could attempt to predict the difference between the actual and expected participation probability, but that isn't quite what we want
            // The difference is that there's an important difference between probability 0.01 and probability 0.1, whereas the difference between 0.5 and 1 is not so important

            // Need a function f(numSkips, numParticipations, p) such that:
            //   f is minimized when p = (numParticipations + 1) / (numSkips + numParticipations + 2)
            //   it is always better for a prediction to move closer to the right answer (it's not acceptable to average all the predictions and compare them to the final average)
            // abs(f') is large when numParticipations / (numSkips + numParticipations) is small
            //// previously I've been using (1 - p)^2 * participationFraction + (p)^2 * skipFraction
            //// Note df/dp = -2 (1 - p) * participationFraction + 2 * p * (1 - participationFraction) = 2 * p * participationFraction - 2 * participationFraction - 2 * p * participationFraction + 2 * p
            //// Which is zero when p = participationFraction
            // Consider this error function:
            //   If the user did the activity, then the score is -1 / p
            //   If the user did not do the activity, then the score is -ln(p)
            //// Then, the total score = -numParticipations / p + numSkips * -ln(p)
            //// Which has derivative = numParticipations / (p^2) + -numSkips / p = (1/p^2) * (numParticipations - numSkips * p)
            //// Which clearly is zero when p = numParticipations / numSkips, although unfortunately that's not quite the goal (but it's close)
            // Consider this error function"
            //   If the user did the activity, then the score is -1 / p - ln(p)
            //   If the user did not do the activity, then the score is -ln(p)
            //// Then, the total score = -numParticipations / p + numTrials * -ln(p)
            //// Which has derivative = numParticipations / (p^2) - numTrials / p = (p^2) * (numParticipations - numTrials * p)
            //// Which clearly is zero when p = numParticipations / numTrials
            //   Note, however, that this function causes the best score to be limited by the outcomes being predicted: if the user always does the activity, we can't get average score more than 0
            //   So, we add a flipped version of this function, too:
            // The final function, then, is:
            //   If the user did the activity, then the score is -1 / p - ln(p) - ln(1 - p)
            //   If the user did not do the activity, then the score is -1 / (1 - p) - ln(p) - ln(1 - p)

            if (predictedProbability != 0)
            {
                // This is the component that we care about, because it gives stronger weight to cases where the probability is very small
                double scoreComponent = -Math.Log(predictedProbability) + -actualIntensity / predictedProbability;
                this.participationPrediction_score = this.participationPrediction_score.Plus(Distribution.MakeDistribution(scoreComponent, 0, 1));
            }
            if (predictedProbability != 1)
            {
                // This component is added for symmetry, so it is possible to get an arbitrarily large score even if the user did do the activity
                double scoreComponent = -Math.Log(1 - predictedProbability) + (actualIntensity - 1) / (1 - predictedProbability);
                this.participationPrediction_score = this.participationPrediction_score.Plus(Distribution.MakeDistribution(scoreComponent, 0, 1));
            }
        }

        public void Update_ShortTermEfficiency_Error(ActivityDescriptor descriptor, DateTime when, double actualEfficiency)
        {
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            Distribution prediction = this.engine.PredictEfficiency(activity, when);
            this.shortTermEfficiency_error.Add(when, actualEfficiency, prediction);
        }

        private PredictionErrors Compute_FutureEstimateIfSuggested_Errors()
        {
            return this.Compute_FuturePredictions_Error(this.valueIfSuggested_predictions, this.ratingSummarizer, 0, 1);
        }
        private PredictionErrors Compute_FutureEstimateIfParticipated_Errors()
        {
            return this.Compute_FuturePredictions_Error(this.valueIfParticipated_predictions, this.ratingSummarizer, 0, 1);
        }
        private PredictionErrors Compute_FutureEfficiencyIfParticipated_Errors()
        {
            return this.Compute_FuturePredictions_Error(this.efficiencyIfParticipated_predictions, this.efficiencySummarizer, 0, double.PositiveInfinity);
        }

        private PredictionErrors Compute_FuturePredictions_Error(Dictionary<Prediction, ScoreSummary> predictions, ExponentialRatingSummarizer ratingSummarizer, double minAllowedValue, double maxAllowedValue)
        {
            PredictionErrors result = new LongtermPredictionErrors(ratingSummarizer.HalfLife, minAllowedValue, maxAllowedValue);

            DateTime maxObservedDate = new DateTime();
            foreach (Prediction prediction in predictions.Keys)
            {
                if (prediction.CreationDate.CompareTo(maxObservedDate) > 0)
                    maxObservedDate = prediction.CreationDate;
            }
            DateTime lastDateToInclude = maxObservedDate.Subtract(TimeSpan.FromDays(7));

            foreach (Prediction prediction in predictions.Keys)
            {
                if (prediction.ApplicableDate.CompareTo(lastDateToInclude) > 0)
                {
                    // When estimating the error of the predictions about the future of the past,
                    // skip any predictions that happened too recently for us to know much about their futures
                    continue;
                }
                ScoreSummary summary = predictions[prediction];
                summary.Update(ratingSummarizer);
                if (summary.Item.Weight > 0)
                {
                    double error = Math.Abs(prediction.Distribution.Mean - summary.Item.Mean);
                    /*if (error > 0.4)
                    {
                        System.Diagnostics.Debug.WriteLine("Surprisingly large error: predicted " + prediction.Distribution + ", true future " + summary.Item + " at " + prediction.ApplicableDate);
                    }*/
                    result.Add(prediction.ApplicableDate, summary.Item.Mean, prediction.Distribution);
                }
            }
            return result;
        }

        public EngineTesterResults Results
        {
            get
            {
                EngineTesterResults results = new EngineTesterResults();
                results.Longterm_PredictionIfSuggested_Error = this.Compute_FutureEstimateIfSuggested_Errors();
                results.Longterm_PredictionIfParticipated_Error = this.Compute_FutureEstimateIfParticipated_Errors();
                results.Longterm_EfficiencyIfPredicted_Error = this.Compute_FutureEfficiencyIfParticipated_Errors();
                results.TypicalEfficiencyError = this.shortTermEfficiency_error;
                
                // how well the score prediction does
                results.TypicalScoreError = this.shortTermScore_error;

                // Compute how well the probability prediction does (weighting smaller probabilities more heavily)
                //
                // We want to calculate the effective predictedProbability that would give us the value of participationPrediction_score.Mean that we have
                //
                // The formula by which we calculate participationPrediction_score.Mean is:
                // participationPrediction_score.Mean = 0.5 * (-Math.Log(predictedProbability) + -actualIntensity / predictedProbability + -Math.Log(1 - predictedProbability) + (actualIntensity - 1) / (1 - predictedProbability))
                //
                // The question is, given a particular score mean, what value of predictedProbability would a rational model have to output to produce that score?
                // Note that if a rational model outputs a certain value for predictedProbability, then it means that the value of actualIntensity will match that value, on average
                // That gives us:
                // participationPrediction_score.Mean = 0.5 * (-Math.Log(predictedProbability) + -Math.Log(1 - predictedProbability) - 2)
                // 2 * participationPrediction_score.Mean + 2 = (-Math.Log(predictedProbability * (1 - predictedProbability)))
                // 2 * this.participationPrediction_score.Mean + 2 = (-Math.Log(predictedProbability * (1 - predictedProbability)))
                // predictedProbability * (1 - predictedProbability) = e ^ (-2 * this.participationPrediction_score.Mean - 2);
                // X * X - X + e ^ (-2 * this.participationPrediction_score.Mean - 2) = 0
                // X = (1 + sqrt(1 - 4 * e ^ (-2 * this.participationPrediction_score.Mean - 2))) / 2;
                results.TypicalProbability = Math.Round((1 + Math.Sqrt(1 - 4 * Math.Exp(-2 * this.participationPrediction_score.Mean - 2))) / 2, 4);
                results.ParticipationHavingMostSurprisingScore = this.mostSurprisingParticipation;
                return results;
            }
        }

        public void PrintResults()
        {
            EngineTesterResults results = this.Results;

            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine(results.Longterm_PredictionIfSuggested_Error + ", longtermHappinessIfSuggested");
            System.Diagnostics.Debug.WriteLine(results.Longterm_PredictionIfParticipated_Error + ", longtermHappinessIfParticipated");
            System.Diagnostics.Debug.WriteLine(results.TypicalScoreError + ", score");
            System.Diagnostics.Debug.WriteLine(Math.Round(results.TypicalProbability, 4) + ",                                         equivalentWeightedProbability");
            System.Diagnostics.Debug.WriteLine(results.TypicalEfficiencyError + ", efficiency");
            System.Diagnostics.Debug.WriteLine(results.Longterm_EfficiencyIfPredicted_Error + ", longtermEfficiencyIfParticipated");
        }

        private void PrintFinalResults()
        {
            this.PrintResults();
            DateTime executionEnd = DateTime.Now;
            TimeSpan duration = executionEnd.Subtract(this.executionStart);
            System.Diagnostics.Debug.WriteLine("EngineTester completed in " + duration);
            System.Diagnostics.Debug.WriteLine("");
        }
        public override void Finish()
        {
            this.PrintFinalResults();
        }

        public Distribution SquaredParticipationProbabilityError
        {
            get
            {
                return this.squaredParticipationProbabilityError;
            }
        }
        public DateTime? EarliestRatingDate { get; set; }

        private PredictionErrors shortTermScore_error = new ShorttermPredictionErrors(0, 1);
        private Distribution squared_longTermValue_error = new Distribution();
        private Distribution squaredParticipationProbabilityError = new Distribution();
        private Distribution participationPrediction_score = new Distribution();
        private PredictionErrors shortTermEfficiency_error = new ShorttermPredictionErrors(0, double.PositiveInfinity);
        private ParticipationSurprise mostSurprisingParticipation;
        private ExponentialRatingSummarizer ratingSummarizer;
        private ExponentialRatingSummarizer efficiencySummarizer;
        // the Engine predicts longterm value based on its suggestions. valueIfSuggested_predictions maps the predictions made to the actual observed longterm value
        private Dictionary<Prediction, ScoreSummary> valueIfSuggested_predictions = new Dictionary<Prediction, ScoreSummary>();
        // the Engine predicts longterm value based on what the user participates in. valueIfParticipated_predictions maps the predictions made to the actual observed longterm value
        private Dictionary<Prediction, ScoreSummary> valueIfParticipated_predictions = new Dictionary<Prediction, ScoreSummary>();
        // the Engine predicts longterm efficiency based on what the user participates in. efficiencyIfParticipated_predictions maps the predictions made to the actual observed longterm efficiency
        private Dictionary<Prediction, ScoreSummary> efficiencyIfParticipated_predictions = new Dictionary<Prediction, ScoreSummary>();
        private int numRatings;
        private DateTime executionStart;
    }

    public class EngineTesterResults
    {
        // overall error in (the estimated and actual (longterm happiness prediction if (the given activity is suggested)))
        public PredictionErrors Longterm_PredictionIfSuggested_Error;
        // overall error in (the estimated and actual (longterm happiness prediction if (the given activity is participated in)))
        public PredictionErrors Longterm_PredictionIfParticipated_Error;
        // typical error in the score prediction
        public PredictionErrors TypicalScoreError;
        // An estimate of the accuracy of the estimates of participation probability.
        // This is the probability such that if the true probability were this value, and if all estimates were perfect, the overall error would be what was observed
        public double TypicalProbability;

        // typical error in the efficiency prediction
        public PredictionErrors TypicalEfficiencyError;
        // overall error in (the estimated and actual (longterm efficiency prediction if (the given activity is participated in)))
        public PredictionErrors Longterm_EfficiencyIfPredicted_Error;

        public ParticipationSurprise ParticipationHavingMostSurprisingScore;
    }

    // A PredictionErrors records errors in Predictions
    // Each Prediction contains a Mean and a StdDev, and a PredictionErrors records the errors in each
    public abstract class PredictionErrors
    {
        public PredictionErrors(double minAllowedValue, double maxAllowedValue)
        {
            this.minAllowedValue = minAllowedValue;
            this.maxAllowedValue = maxAllowedValue;
        }
        public virtual void Add(DateTime when, double correctValue, Distribution prediction)
        {
            // update errorsOfMeansSquared
            double errorOfMean = correctValue - prediction.Mean;
            if (prediction.Mean > this.maxAllowedValue)
            {
                throw new Exception("Prediction too large: " + prediction.Mean);
            }
            if (prediction.Mean < this.minAllowedValue)
            {
                throw new Exception("Prediction too small: " + prediction.Mean);
            }
            if (correctValue > this.maxAllowedValue)
            {
                throw new Exception("True value too large: " + correctValue);
            }
            if (correctValue < this.minAllowedValue)
            {
                throw new Exception("True value too small: " + correctValue);
            }

            this.errorsOfMeansSquared = this.errorsOfMeansSquared.Plus(Distribution.MakeDistribution(errorOfMean * errorOfMean, 0, 1));

            // update errorsOfStdDevsSquared
            double predictedStddev = prediction.StdDev;
            double errorOfStdDev = Math.Abs(errorOfMean) - predictedStddev;
            this.errorsOfStdDevsSquared = this.errorsOfStdDevsSquared.Plus(Distribution.MakeDistribution(errorOfStdDev * errorOfStdDev, 0, 1));

            // update list
            PredictionError error = new PredictionError();
            error.ActualMean = correctValue;
            error.Predicted = prediction;
            error.When = when;
            this.errorList.Add(error);
        }
        public List<PredictionError> All
        {
            get
            {
                return this.errorList;
            }
        }
        public double MinAllowedValue
        {
            get
            {
                return this.minAllowedValue;
            }
        }
        public double MaxAllowedValue
        {
            get
            {
                return this.maxAllowedValue;
            }
        }
        protected double meanErrorsOfMeans
        {
            get
            {
                return Math.Sqrt(this.errorsOfMeansSquared.Mean);
            }
        }
        protected double meanErrorOfStdDevs
        {
            get
            {
                return Math.Sqrt(this.errorsOfStdDevsSquared.Mean);
            }
        }

        private Distribution errorsOfMeansSquared = new Distribution();
        private Distribution errorsOfStdDevsSquared = new Distribution();
        private double minAllowedValue;
        private double maxAllowedValue;
        private List<PredictionError> errorList = new List<PredictionError>();
    }

    public class LongtermPredictionErrors : PredictionErrors
    {
        public LongtermPredictionErrors(TimeSpan halfLife, double minAllowedValue, double maxAllowedValue) : base(minAllowedValue, maxAllowedValue)
        {
            this.halfLife = halfLife;
        }
        public override string ToString()
        {
            return "Means.MeanErr: " + this.formatValue(this.meanErrorsOfMeans) + ", StdDevs.MeanErr: " + this.formatValue(this.meanErrorOfStdDevs);
        }

        // given a double representing the net present value from 0 to 1, format it as a string
        private string formatValue(double value)
        {
            double valueInDays = value * this.halfLife.TotalDays;
            return "" + Math.Round(value, 4) + " (" + Math.Round(valueInDays) + " days)";
        }
        private TimeSpan halfLife;
    }

    public class ShorttermPredictionErrors : PredictionErrors
    {
        public ShorttermPredictionErrors(double minAllowedValue, double maxAllowedValue) : base(minAllowedValue, maxAllowedValue)
        {
        }
        public override string ToString()
        {
            return "Means.MeanErr: " + this.formatValue(this.meanErrorsOfMeans) + ", StdDevs.MeanErr: " + this.formatValue(this.meanErrorOfStdDevs);
        }
        public override void Add(DateTime when, double correctValue, Distribution prediction)
        {
            base.Add(when, correctValue, prediction);

            // update observed
            this.correctValues.Add(correctValue);
        }

        // given a double representing the an error value from 0 to 1, format it as a string
        private string formatValue(double value)
        {
            double meanCorrectValue = this.correctValues.Mean;

            return "" + Math.Round(value, 4) + " (" + Math.Round(value / meanCorrectValue, 4) + " * average)";
        }

        private Distribution correctValues = new Distribution();
    }

    public class ParticipationSurprise
    {
        public double ExpectedRating;
        public double ActualRating;
        public double Surprise;
        public ActivityDescriptor ActivityDescriptor;
        public DateTime Date;
    }

    public class PredictionError
    {
        public DateTime When;
        public Distribution Predicted;
        public double ActualMean;
    }
}