using ActivityRecommendation;
using System;
using Xunit;

namespace UnitTests
{
    public class EngineTest
    {
        [Fact]
        public void Test_ComputeNumRemainingExperimentParticipations_IsConsistent()
        {
            Engine engine = new Engine();
            for (int total = 0; total < 20; total++)
            {
                for (int numCompletedParticipations = 0; numCompletedParticipations < total; numCompletedParticipations++)
                {
                    int expectedNextNumRemainingExperimentParticipations = engine.ComputeNumRemainingExperimentParticipations(numCompletedParticipations, total - numCompletedParticipations) - 1;
                    int actualNextNumRemainingExperimentParticipations = engine.ComputeNumRemainingExperimentParticipations(numCompletedParticipations + 1, total - numCompletedParticipations - 1);
                    if (expectedNextNumRemainingExperimentParticipations >= 0 && actualNextNumRemainingExperimentParticipations != expectedNextNumRemainingExperimentParticipations)
                    {
                        string message = "engine.ComputeNumRemainingExperimentParticipations(" + numCompletedParticipations + ", " + (total - numCompletedParticipations) + ") - 1 = " +
                            + expectedNextNumRemainingExperimentParticipations + " but " +
                            "engine.ComputeNumRemainingExperimentParticipations(" + (numCompletedParticipations + 1) + ", " + (total - numCompletedParticipations - 1) + ") = " +
                            + actualNextNumRemainingExperimentParticipations;
                        throw new Exception(message);
                    }
                }
            }
        }

        [Fact]
        public void Test_ComputeNumRemainingExperimentParticipations_IncreasingSlowly()
        {
            Engine engine = new Engine();
            int result = engine.ComputeNumRemainingExperimentParticipations(0, 10000);
            if (result < 100)
            {
                string message = "engine.ComputeNumRemainingExperimentParticipations(0, 10000) = " + result + " which is too small";
                throw new Exception(message);
            }
            if (result > 10000 - 100)
            {
                string message = "engine.ComputeNumRemainingExperimentParticipations(0, 10000) = " + result + " which is too large";
                throw new Exception(message);
            }
        }
    }
}
