using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdaptiveLinearInterpolation
{
    // The IDatapoint interface requires that the object have coordinates
    public interface IDatapoint
    {
        double[] Coordinates
        {
            get;
        }
        int NumDimensions
        {
            get;
        }
        double Output
        {
            get;
        }
    }
    public class Datapoint : IDatapoint
    {
        static int nextID = 0;
        public Datapoint(double[] startingCoordinates, double startingScore)
        {
            this.coordinates = startingCoordinates;
            this.score = startingScore;
            this.Initialize();
        }
        public Datapoint(double x, double startingScore)
        {
            this.coordinates = new double[1];
            this.coordinates[0] = x;
            this.score = startingScore;
            this.Initialize();
        }
        public Datapoint(double x, double y, double startingScore)
        {
            this.coordinates = new double[2];
            this.coordinates[0] = x;
            this.coordinates[1] = y;
            this.score = startingScore;
            this.Initialize();
        }
        public Datapoint(double x, double y, double z, double startingScore)
        {
            this.coordinates = new double[3];
            this.coordinates[0] = x;
            this.coordinates[1] = y;
            this.coordinates[2] = z;
            this.score = startingScore;
            this.Initialize();
        }
        private void Initialize()
        {
            this.id = nextID;
            nextID++;
        }
        public double[] Coordinates
        {
            get
            {
                return this.coordinates;
            }
        }
        public int NumDimensions
        {
            get
            {
                return this.coordinates.Length;
            }
        }
        public bool InputEquals(Datapoint other)
        {
            int i;
            for (i = 0; i < this.coordinates.Length; i++)
            {
                if (this.coordinates[i] != other.coordinates[i])
                    return false;
            }
            return true;
        }
        public override string ToString()
        {
            string result = "coordinates:(";
            int i;
            for (i = 0; i < this.coordinates.Length; i++)
            {
                result += this.coordinates[i].ToString() + ",";
            }
            result += ") output=" + this.Output.ToString();
            return result;
        }
        public double Output
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
        private double[] coordinates;
        private double score;
        private int id;
    }
}
