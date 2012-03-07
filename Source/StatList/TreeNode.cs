using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// A TreeNode stores the statistics about all of the children of this node
namespace StatLists
{
    class TreeNode<KeyType, ValueType>
    {

        #region Public Member Functions

        public TreeNode(KeyType startingKey, ValueType startingValue)
        {
            this.Stats = new ListItemStats<KeyType, ValueType>(startingKey, startingValue);
            //this.Key = startingKey;
            //this.Value = startingValue;
            this.Aggregate = startingValue;
            this.SubnodeCount = 1;
            this.MaxDepth = 1;
            this.Updated = true;
            this.DepthAtLatestRebalancing = 1;
            this.MaxSubkey = this.MinSubkey = this.Key;
        }
        public ListItemStats<KeyType, ValueType> Stats { get; set; }
        public KeyType Key 
        {
            get
            {
                return this.Stats.Key;
            }
        }
        public ValueType Value 
        {
            get
            {
                return this.Stats.Value;
            }
        }
        public ValueType Aggregate { get; set; }
        public int SubnodeCount { get; set; }   // the number of nodes that are either this node or its descendants
        public bool Updated { get; set; }   // tells whether the Aggregate and SubnodeCount are up-to-date
        public int MaxDepth { get; set; }   // a leaf node has MaxDepth of 1
        public int DepthAtLatestRebalancing { get; set; }   // what the value of MaxDepth was when the tree was last rebalanced
        public KeyType MinSubkey { get; set; }  // the minimum value of all keys among all descendents
        public KeyType MaxSubkey { get; set; }  // the maximum value of all keys among all descendents
        public int GetNumLeftChildren()
        {
            if (this.LeftChild == null)
                return 0;
            return this.LeftChild.SubnodeCount;
        }
        public int GetNumRightChildren()
        {
            if (this.RightChild == null)
                return 0;
            return this.RightChild.SubnodeCount;
        }
        public TreeNode<KeyType, ValueType> LeftChild 
        {
            get
            {
                return this.leftChild;
            }
            set
            {
                // update parent and child pointers
                if (this.LeftChild != null)
                {
                    this.LeftChild.parent = null;
                }
                this.leftChild = value;
                if (value != null)
                {
                    if (value.parent != null)
                    {
                        value.parent.ReplaceChild(value, null);
                    }
                    value.parent = this;
                }
            }
        }
        public TreeNode<KeyType, ValueType> RightChild
        {
            get
            {
                return this.rightChild;
            }
            set
            {
                // update parent and child pointers
                if (this.RightChild != null)
                {
                    this.RightChild.parent = null;
                }
                this.rightChild = value;
                if (value != null)
                {
                    if (value.parent != null)
                    {
                        value.parent.ReplaceChild(value, null);
                    }
                    value.parent = this;
                }
            }
        }
        public TreeNode<KeyType, ValueType> Parent
        {
            get
            {
                return this.parent;
            }
        }
       
        public void ReplaceChild(TreeNode<KeyType, ValueType> oldChild, TreeNode<KeyType, ValueType> newChild)
        {
            if (this.LeftChild == oldChild)
            {
                this.LeftChild = newChild;
            }
            if (this.RightChild == oldChild)
            {
                this.RightChild = newChild;
            }
        }


        #endregion

        #region Private Member Variables

        private TreeNode<KeyType, ValueType> leftChild;
        private TreeNode<KeyType, ValueType> rightChild;
        private TreeNode<KeyType, ValueType> parent;

        #endregion
    }
}
