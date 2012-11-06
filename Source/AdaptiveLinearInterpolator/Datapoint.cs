using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdaptiveLinearInterpolation
{
    // The IDatapoint interface requires that the object have coordinates
    public interface IDatapoint<ScoreType>
    {
        double[] InputCoordinates
        {
            get;
        }
        int NumInputDimensions
        {
            get;
        }
        ScoreType Score
        {
            get;
        }
        double[] OutputCoordinates
        {
            get;
        }
    }
    public class Datapoint<ScoreType> : IDatapoint<ScoreType>
    {
        static int nextID = 0;
        public Datapoint(double[] inputs, ScoreType startingScore)
        {
            this.inputCoordinates = inputs;
            this.score = startingScore;
            this.Initialize();
        }
        public Datapoint(double input1, ScoreType startingScore)
        {
            this.inputCoordinates = new double[1];
            this.inputCoordinates[0] = input1;
            this.score = startingScore;
            this.Initialize();
        }
        public Datapoint(double input1, double input2, ScoreType startingScore)
        {
            this.inputCoordinates = new double[2];
            this.inputCoordinates[0] = input1;
            this.inputCoordinates[1] = input2;
            this.score = startingScore;
            this.Initialize();
        }
        public Datapoint(double input1, double input2, double input3, ScoreType startingScore)
        {
            this.inputCoordinates = new double[3];
            this.inputCoordinates[0] = input1;
            this.inputCoordinates[1] = input2;
            this.inputCoordinates[2] = input3;
            this.score = startingScore;
            this.Initialize();
        }
        public Datapoint(double[] inputs, double[] outputs, ScoreType startingScore)
        {
            this.inputCoordinates = inputs;
            this.outputCoordinates = outputs;
            this.score = startingScore;
            this.Initialize();
        }
        private void Initialize()
        {
            this.id = nextID;
            nextID++;
        }
        public double[] InputCoordinates
        {
            get
            {
                return this.inputCoordinates;
            }
        }
        public int NumInputDimensions
        {
            get
            {
                return this.inputCoordinates.Length;
            }
        }
        public double[] OutputCoordinates
        {
            get
            {
                return this.outputCoordinates;
            }
        }
        public bool InputEquals(Datapoint<ScoreType> other)
        {
            int i;
            for (i = 0; i < this.inputCoordinates.Length; i++)
            {
                if (this.inputCoordinates[i] != other.InputCoordinates[i])
                    return false;
            }
            return true;
        }
        public override string ToString()
        {
            string result = "coordinates:(";
            int i;
            for (i = 0; i < this.inputCoordinates.Length; i++)
            {
                result += this.inputCoordinates[i].ToString() + ",";
            }
            result += ") score=" + this.Score.ToString();
            return result;
        }
        public ScoreType Score
        {
            get
            {
                return this.score;
            }
        }
        public int ID
        {
            get
            {
                return this.id;
            }
        }
        private double[] inputCoordinates;
        private double[] outputCoordinates;
        private ScoreType score;
        private int id;
    }
}
